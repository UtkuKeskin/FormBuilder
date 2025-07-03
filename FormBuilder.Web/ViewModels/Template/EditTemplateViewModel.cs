namespace FormBuilder.Web.ViewModels.Template
{
    public class EditTemplateViewModel : CreateTemplateViewModel
    {
        public int Id { get; set; }
        public string? CurrentImageUrl { get; set; }
        public int Version { get; set; } 

    }
}