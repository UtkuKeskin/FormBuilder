namespace FormBuilder.Web.ViewModels.Admin
{
    public class AdminTemplatesViewModel
    {
        public List<AdminTemplateItemViewModel> Templates { get; set; } = new();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public string SearchTerm { get; set; }
    }

    public class AdminTemplateItemViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string AuthorEmail { get; set; }
        public string AuthorId { get; set; }
        public bool IsPublic { get; set; }
        public int FormCount { get; set; }
        public int LikeCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public string TopicName { get; set; }
    }
}