using System;
using System.Collections.Generic;

namespace FormBuilder.Web.ViewModels.Template
{
    public class TemplateViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public string TopicName { get; set; }
        public string AuthorName { get; set; }
        public string AuthorId { get; set; }
        public bool IsPublic { get; set; }
        public int FormCount { get; set; }
        public int LikeCount { get; set; }
        public bool IsLikedByCurrentUser { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<string> Tags { get; set; } = new();
        
        // For list display
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
        
        // Questions
        public List<QuestionViewModel> Questions { get; set; } = new();
    }
    
    public class QuestionViewModel
    {
        public string Type { get; set; } // string, text, integer, checkbox
        public string Question { get; set; }
        public string Description { get; set; }
        public bool ShowInTable { get; set; }
        public int Order { get; set; }
    }
}