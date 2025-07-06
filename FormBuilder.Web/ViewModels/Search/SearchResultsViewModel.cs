namespace FormBuilder.Web.ViewModels.Search
{
    public class SearchResultsViewModel
    {
        public string Query { get; set; }
        public List<SearchItemViewModel> Results { get; set; } = new();
        public int TotalResults { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public long SearchTimeMs { get; set; }
        public string SortBy { get; set; }
        public int? TopicFilter { get; set; }
        public string TimeFilter { get; set; }
        public string ResponseFilter { get; set; }
        
        public int TotalPages => (int)Math.Ceiling(TotalResults / (double)PageSize);
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
    }

    public class SearchItemViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string AuthorName { get; set; }
        public string TopicName { get; set; }
        public bool IsPublic { get; set; }
        public int FormCount { get; set; }
        public int LikeCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<string> Tags { get; set; } = new();
        public bool CanAccess { get; set; }
    }
}