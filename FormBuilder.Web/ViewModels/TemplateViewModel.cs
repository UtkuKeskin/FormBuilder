namespace FormBuilder.Web.ViewModels
{
    public class TemplateViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public string AuthorName { get; set; }
        public string TopicName { get; set; }
        public int FormCount { get; set; }
        public int LikeCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsPublic { get; set; }
    }
}