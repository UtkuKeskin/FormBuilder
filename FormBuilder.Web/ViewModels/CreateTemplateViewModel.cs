using System.ComponentModel.DataAnnotations;

namespace FormBuilder.Web.ViewModels
{
    public class CreateTemplateViewModel
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [Required]
        [StringLength(1000)]
        public string Description { get; set; }

        [Required]
        public int TopicId { get; set; }

        public bool IsPublic { get; set; } = true;

        public IFormFile? ImageFile { get; set; }

        // Question fields will be added dynamically
    }
}