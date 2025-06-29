namespace FormBuilder.Web.ViewModels
{
    public class UserViewModel
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public bool IsAdmin { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public string Theme { get; set; }
        public string Language { get; set; }
    }
}