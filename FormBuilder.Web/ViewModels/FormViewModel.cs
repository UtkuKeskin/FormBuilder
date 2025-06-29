namespace FormBuilder.Web.ViewModels
{
    public class FormViewModel
    {
        public int Id { get; set; }
        public string TemplateName { get; set; }
        public DateTime FilledAt { get; set; }
        public Dictionary<string, object> Answers { get; set; }
    }
}