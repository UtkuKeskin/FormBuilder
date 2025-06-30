using System.Collections.Generic;

namespace FormBuilder.Web.ViewModels.Template
{
    public class TemplateListViewModel
    {
        public List<TemplateViewModel> Templates { get; set; } = new();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public string SortBy { get; set; }
        public string SortOrder { get; set; }
        public string SearchTerm { get; set; }
        public int? TopicFilter { get; set; }
        
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
    }
}