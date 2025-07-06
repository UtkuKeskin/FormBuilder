using System.Collections.Generic;
using System.Threading.Tasks;

namespace FormBuilder.Core.Interfaces
{
    public interface ISearchService
    {
        Task<SearchResult> SearchTemplatesAsync(SearchParameters parameters);
        Task<IEnumerable<string>> GetSearchSuggestionsAsync(string query);
    }

    public class SearchParameters
    {
        public string Query { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SortBy { get; set; } = "relevance";
        public int? TopicId { get; set; }
        public bool IncludePrivate { get; set; } = false;

        public string TimeFilter { get; set; } // today, week, month
        public string ResponseFilter { get; set; } // none, popular
        public List<string> Tags { get; set; } = new();
    }

    public class SearchResult
    {
        public List<SearchResultItem> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public string Query { get; set; }
        public long ElapsedMs { get; set; }
    }

    public class SearchResultItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string AuthorName { get; set; }
        public string AuthorId { get; set; }
        public string TopicName { get; set; }
        public bool IsPublic { get; set; }
        public int FormCount { get; set; }
        public int LikeCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<string> Tags { get; set; } = new();
        public string HighlightedTitle { get; set; }
        public string HighlightedDescription { get; set; }
        public float Relevance { get; set; }
    }
}