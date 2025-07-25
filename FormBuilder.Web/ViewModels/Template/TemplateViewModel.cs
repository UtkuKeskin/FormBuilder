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
        
        // Permission properties
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
        public bool CanAccess { get; set; }
        
        // Questions
        public List<QuestionViewModel> Questions { get; set; } = new();
        
        // Forms collection for Results tab
        public List<FormViewModel> Forms { get; set; } = new();
    }

    // Form view model for template details
    public class FormViewModel
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public DateTime FilledAt { get; set; }
        
        public Dictionary<string, string> DisplayAnswers { get; set; } = new();
    }
    
    public class QuestionViewModel
    {
        public string Type { get; set; }
        public string Question { get; set; }
        public string Description { get; set; }
        public bool ShowInTable { get; set; }
        public int Order { get; set; }
    }
}