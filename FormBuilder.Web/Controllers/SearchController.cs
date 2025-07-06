using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FormBuilder.Core.Interfaces;
using FormBuilder.Web.ViewModels.Search;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace FormBuilder.Web.Controllers
{
    public class SearchController : Controller
    {
        private readonly ISearchService _searchService;
        private readonly ITemplateService _templateService;
        private readonly ILogger<SearchController> _logger;

        public SearchController(
            ISearchService searchService,
            ITemplateService templateService,
            ILogger<SearchController> logger)
        {
            _searchService = searchService;
            _templateService = templateService;
            _logger = logger;
        }

        // GET: /Search/Results
        [AllowAnonymous]
        public async Task<IActionResult> Results(
            string q, 
            int page = 1, 
            string sort = "relevance",
            int? topic = null,
            string time = null,     // Yeni
            string responses = null, // Yeni  
            string tags = null)      // Yeni
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                ViewBag.Topics = await GetTopicsAsync();
                return View(new SearchResultsViewModel { Query = "" });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAuthenticated = User.Identity?.IsAuthenticated ?? false;

            var parameters = new SearchParameters
            {
                Query = q,
                Page = page,
                PageSize = 10,
                SortBy = sort,
                TopicId = topic,
                IncludePrivate = false, // Only public for anonymous
                TimeFilter = time,
                ResponseFilter = responses,
                Tags = string.IsNullOrEmpty(tags) ? new() : tags.Split(',').ToList()
            };

            var result = await _searchService.SearchTemplatesAsync(parameters);

            var model = new SearchResultsViewModel
            {
                Query = q,
                Results = result.Items.Select(item => new SearchItemViewModel
                {
                    Id = item.Id,
                    Title = item.HighlightedTitle,
                    Description = item.HighlightedDescription,
                    AuthorName = item.AuthorName,
                    TopicName = item.TopicName,
                    FormCount = item.FormCount,
                    LikeCount = item.LikeCount,
                    CreatedAt = item.CreatedAt,
                    Tags = item.Tags,
                    IsPublic = item.IsPublic,
                    CanAccess = CanUserAccess(item, userId, isAuthenticated)
                }).ToList(),
                TotalResults = result.TotalCount,
                CurrentPage = result.CurrentPage,
                PageSize = result.PageSize,
                SearchTimeMs = result.ElapsedMs,
                SortBy = sort,
                TopicFilter = topic,
                TimeFilter = time,        // Yeni
                ResponseFilter = responses // Yeni
            };

            ViewBag.Topics = await GetTopicsAsync();
            
            _logger.LogInformation(
                "Search completed: Query={Query}, Results={Count}, Time={Time}ms", 
                q, result.TotalCount, result.ElapsedMs);

            return View(model);
        }

        // GET: /Search/Suggestions
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Suggestions(string q)
        {
            if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
            {
                return Json(new List<string>());
            }

            var suggestions = await _searchService.GetSearchSuggestionsAsync(q);
            return Json(suggestions);
        }

        // Private helper methods
        private bool CanUserAccess(
            SearchResultItem item, 
            string userId, 
            bool isAuthenticated)
        {
            // Public templates are accessible to all
            if (item.IsPublic) return true;
            
            // Private templates need auth
            if (!isAuthenticated) return false;
            
            // Owner can always access
            return item.AuthorId == userId;
        }

        // - Use existing TemplateService
        private async Task<List<FormBuilder.Core.Entities.Topic>> GetTopicsAsync()
        {
            return await _templateService.GetTopicsAsync();
        }
    }
}