namespace FormBuilder.Web.ViewModels.Admin
{
    public class UsersViewModel
    {
        public List<UserItemViewModel> Users { get; set; } = new();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public string SearchTerm { get; set; }
    }

    public class UserItemViewModel
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsLocked { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public int TemplateCount { get; set; }
        public int FormCount { get; set; }
    }
}