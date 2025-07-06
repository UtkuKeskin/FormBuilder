using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using FormBuilder.Core.Entities;
using FormBuilder.Core.Interfaces;
using FormBuilder.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace FormBuilder.Infrastructure.Services
{
    public class SearchService : ISearchService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly ILogger<SearchService> _logger;
        private const int CACHE_MINUTES = 5;

        public SearchService(
            ApplicationDbContext context, 
            IMemoryCache cache,
            ILogger<SearchService> logger)
        {
            _context = context;
            _cache = cache;
            _logger = logger;
        }

        public async Task<SearchResult> SearchTemplatesAsync(SearchParameters parameters)
        {
            var sw = Stopwatch.StartNew();
            
            // Clean search query
            var cleanQuery = CleanSearchQuery(parameters.Query);
            if (string.IsNullOrWhiteSpace(cleanQuery))
            {
                return CreateEmptyResult(parameters);
            }

            // Check cache
            var cacheKey = GenerateCacheKey(parameters);
            if (_cache.TryGetValue<SearchResult>(cacheKey, out var cached))
            {
                _logger.LogDebug("Search cache hit: {Query}", cleanQuery);
                return cached;
            }

            // Execute search
            var result = await ExecuteSearchAsync(cleanQuery, parameters);
            
            result.ElapsedMs = sw.ElapsedMilliseconds;
            
            // Cache result
            _cache.Set(cacheKey, result, TimeSpan.FromMinutes(CACHE_MINUTES));
            
            return result;
        }

        private async Task<SearchResult> ExecuteSearchAsync(
            string query, 
            SearchParameters parameters)
        {
            // Build base query
            var templatesQuery = BuildSearchQuery(query, parameters);
            
            // Apply sorting AFTER includes
            templatesQuery = ApplySorting(templatesQuery, parameters.SortBy, query);
            
            // Get total count
            var totalCount = await templatesQuery.CountAsync();
            
            // Apply pagination and fetch
            var items = await ApplyPaginationAsync(templatesQuery, parameters);
            
            // Process results
            var resultItems = ProcessSearchResults(items, query);
            
            return new SearchResult
            {
                Items = resultItems,
                TotalCount = totalCount,
                CurrentPage = parameters.Page,
                PageSize = parameters.PageSize,
                Query = query
            };
        }

        private IQueryable<Template> BuildSearchQuery(
            string query, 
            SearchParameters parameters)
        {
            var baseQuery = _context.Templates
                .AsNoTracking()
                .Where(t => t.IsPublic || parameters.IncludePrivate);

            // Apply PostgreSQL full-text search
            if (!string.IsNullOrEmpty(query))
            {
                baseQuery = ApplyFullTextSearch(baseQuery, query);
            }

            // Apply topic filter
            if (parameters.TopicId.HasValue)
            {
                baseQuery = baseQuery.Where(t => t.TopicId == parameters.TopicId.Value);
            }

            // Apply time filter
            if (!string.IsNullOrEmpty(parameters.TimeFilter))
            {
                var cutoffDate = parameters.TimeFilter switch
                {
                    "today" => DateTime.UtcNow.Date,
                    "week" => DateTime.UtcNow.AddDays(-7),
                    "month" => DateTime.UtcNow.AddMonths(-1),
                    _ => DateTime.MinValue
                };

                if (cutoffDate > DateTime.MinValue)
                {
                    baseQuery = baseQuery.Where(t => t.CreatedAt >= cutoffDate);
                }
            }

            // Apply response filter
            if (!string.IsNullOrEmpty(parameters.ResponseFilter))
            {
                baseQuery = parameters.ResponseFilter switch
                {
                    "none" => baseQuery.Where(t => t.Forms.Count == 0),
                    "popular" => baseQuery.Where(t => t.Forms.Count >= 5),
                    _ => baseQuery
                };
            }

            // Apply tag filter
            if (parameters.Tags?.Any() == true)
            {
                baseQuery = baseQuery.Where(t => 
                    t.TemplateTags.Any(tt => parameters.Tags.Contains(tt.Tag.Name)));
            }

            // Include related data
            baseQuery = baseQuery
                .Include(t => t.User)
                .Include(t => t.Topic)
                .Include(t => t.Forms)
                .Include(t => t.Likes)
                .Include(t => t.TemplateTags)
                    .ThenInclude(tt => tt.Tag);

            return baseQuery;
        }

        // IMPROVED VERSION WITH FALLBACK
        private IQueryable<Template> ApplyFullTextSearch(
            IQueryable<Template> query, 
            string searchQuery)
        {
            try
            {
                // Try PostgreSQL full-text search first
                var tsQuery = FormatTsQuery(searchQuery);
                
                return query.Where(t => EF.Functions.ToTsVector("english", t.Title + " " + t.Description)
                    .Matches(EF.Functions.PlainToTsQuery("english", tsQuery)));
            }
            catch (Exception ex)
            {
                // Fallback to LIKE search if full-text fails
                _logger.LogWarning(ex, "Full-text search failed, falling back to LIKE search");
                return ApplyLikeSearch(query, searchQuery);
            }
        }

        // FALLBACK METHOD
        private IQueryable<Template> ApplyLikeSearch(IQueryable<Template> query, string searchQuery)
        {
            var lowerQuery = searchQuery.ToLower();
            return query.Where(t => 
                t.Title.ToLower().Contains(lowerQuery) || 
                t.Description.ToLower().Contains(lowerQuery) ||
                t.User.UserName.ToLower().Contains(lowerQuery));
        }

        private async Task<List<Template>> ApplyPaginationAsync(
            IQueryable<Template> query, 
            SearchParameters parameters)
        {
            return await query
                .Skip((parameters.Page - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .ToListAsync();
        }

        private List<SearchResultItem> ProcessSearchResults(
            List<Template> templates, 
            string query)
        {
            return templates.Select(t => new SearchResultItem
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                AuthorName = t.User?.UserName ?? "Unknown",
                AuthorId = t.UserId,
                TopicName = t.Topic?.Name ?? "Other",
                IsPublic = t.IsPublic,
                FormCount = t.Forms?.Count ?? 0,
                LikeCount = t.Likes?.Count ?? 0,
                CreatedAt = t.CreatedAt,
                Tags = t.TemplateTags?.Select(tt => tt.Tag.Name).ToList() ?? new(),
                HighlightedTitle = HighlightText(t.Title, query),
                HighlightedDescription = HighlightText(t.Description, query),
                Relevance = CalculateRelevance(t, query)
            }).ToList();
        }

        public async Task<IEnumerable<string>> GetSearchSuggestionsAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
                return Enumerable.Empty<string>();

            var suggestions = await _context.Templates
                .Where(t => t.IsPublic && t.Title.Contains(query))
                .Select(t => t.Title)
                .Distinct()
                .Take(5)
                .ToListAsync();

            return suggestions;
        }

        // Helper methods
        private string CleanSearchQuery(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return "";
            
            // Remove special chars
            return System.Text.RegularExpressions.Regex
                .Replace(query.Trim(), @"[^\w\s-]", " ")
                .Trim();
        }

        private string GenerateCacheKey(SearchParameters parameters)
        {
            return $"search_{parameters.Query}_{parameters.Page}_{parameters.PageSize}_{parameters.TopicId}";
        }

        private SearchResult CreateEmptyResult(SearchParameters parameters)
        {
            return new SearchResult
            {
                Items = new List<SearchResultItem>(),
                TotalCount = 0,
                CurrentPage = parameters.Page,
                PageSize = parameters.PageSize,
                Query = parameters.Query ?? ""
            };
        }

        private string FormatTsQuery(string query)
        {
            var words = query.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return string.Join(" & ", words);
        }

        private IQueryable<Template> ApplySorting(
            IQueryable<Template> query, 
            string sortBy,
            string searchQuery)
        {
            return sortBy?.ToLower() switch
            {
                "date" => query.OrderByDescending(t => t.CreatedAt),
                "title" => query.OrderBy(t => t.Title),
                "popular" => query.OrderByDescending(t => t.Forms.Count)
                             .ThenByDescending(t => t.Likes.Count),
                "relevance" => string.IsNullOrEmpty(searchQuery) ? 
                    query.OrderByDescending(t => t.CreatedAt) : 
                    query, // Full-text search handles relevance
                _ => query.OrderByDescending(t => t.CreatedAt) // Default to newest
            };
        }

        private string HighlightText(string text, string query)
        {
            if (string.IsNullOrEmpty(text)) return "";
            
            foreach (var word in query.Split(' '))
            {
                if (word.Length < 2) continue;
                
                var pattern = $@"\b({System.Text.RegularExpressions.Regex.Escape(word)})\b";
                text = System.Text.RegularExpressions.Regex.Replace(
                    text, pattern, "<mark>$1</mark>", 
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            }
            
            return text;
        }

        private float CalculateRelevance(Template template, string query)
        {
            float score = 0;
            var lowerQuery = query.ToLower();
            
            // Title match (highest weight)
            if (template.Title.ToLower().Contains(lowerQuery))
                score += 10;
                
            // Description match
            if (template.Description?.ToLower().Contains(lowerQuery) ?? false)
                score += 5;
                
            // Tag match
            if (template.TemplateTags?.Any(tt => 
                tt.Tag.Name.ToLower().Contains(lowerQuery)) ?? false)
                score += 3;
                
            return score;
        }
    }
}