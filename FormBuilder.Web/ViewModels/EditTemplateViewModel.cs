namespace FormBuilder.Web.ViewModels
{
    public class EditTemplateViewModel : CreateTemplateViewModel
    {
        public int Id { get; set; }
        public string? CurrentImageUrl { get; set; }
        public int Version { get; set; }
    }
}