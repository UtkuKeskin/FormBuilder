using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace FormBuilder.Web.ViewModels.Template
{
    public class CreateTemplateViewModel
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, MinimumLength = 3)]
        public string Title { get; set; }

        [Required(ErrorMessage = "Description is required")]
        [StringLength(1000)]
        public string Description { get; set; }

        [Required(ErrorMessage = "Topic is required")]
        public int TopicId { get; set; }

        public IFormFile? ImageFile { get; set; } // Nullable

        public string? Tags { get; set; } // Nullable

        public bool IsPublic { get; set; } = true;

        public List<int>? AllowedUserIds { get; set; } // Nullable

        // Question fields
        // Single-line strings
        public bool CustomString1State { get; set; }
        public string? CustomString1Question { get; set; }
        public string? CustomString1Description { get; set; }
        public bool CustomString1ShowInTable { get; set; }
        public bool CustomString1Required { get; set; }

        public bool CustomString2State { get; set; }
        public string? CustomString2Question { get; set; }
        public string? CustomString2Description { get; set; }
        public bool CustomString2ShowInTable { get; set; }
        public bool CustomString2Required { get; set; }

        public bool CustomString3State { get; set; }
        public string? CustomString3Question { get; set; }
        public string? CustomString3Description { get; set; }
        public bool CustomString3ShowInTable { get; set; }
        public bool CustomString3Required { get; set; }

        public bool CustomString4State { get; set; }
        public string? CustomString4Question { get; set; }
        public string? CustomString4Description { get; set; }
        public bool CustomString4ShowInTable { get; set; }
        public bool CustomString4Required { get; set; }

        // Multi-line texts
        public bool CustomText1State { get; set; }
        public string? CustomText1Question { get; set; }
        public string? CustomText1Description { get; set; }
        public bool CustomText1ShowInTable { get; set; }
        public bool CustomText1Required { get; set; }

        public bool CustomText2State { get; set; }
        public string? CustomText2Question { get; set; }
        public string? CustomText2Description { get; set; }
        public bool CustomText2ShowInTable { get; set; }
        public bool CustomText2Required { get; set; }

        public bool CustomText3State { get; set; }
        public string? CustomText3Question { get; set; }
        public string? CustomText3Description { get; set; }
        public bool CustomText3ShowInTable { get; set; }
        public bool CustomText3Required { get; set; }

        public bool CustomText4State { get; set; }
        public string? CustomText4Question { get; set; }
        public string? CustomText4Description { get; set; }
        public bool CustomText4ShowInTable { get; set; }
        public bool CustomText4Required { get; set; }

        // Integers
        public bool CustomInt1State { get; set; }
        public string? CustomInt1Question { get; set; }
        public string? CustomInt1Description { get; set; }
        public bool CustomInt1ShowInTable { get; set; }
        public bool CustomInt1Required { get; set; }

        public bool CustomInt2State { get; set; }
        public string? CustomInt2Question { get; set; }
        public string? CustomInt2Description { get; set; }
        public bool CustomInt2ShowInTable { get; set; }
        public bool CustomInt2Required { get; set; }

        public bool CustomInt3State { get; set; }
        public string? CustomInt3Question { get; set; }
        public string? CustomInt3Description { get; set; }
        public bool CustomInt3ShowInTable { get; set; }
        public bool CustomInt3Required { get; set; }

        public bool CustomInt4State { get; set; }
        public string? CustomInt4Question { get; set; }
        public string? CustomInt4Description { get; set; }
        public bool CustomInt4ShowInTable { get; set; }
        public bool CustomInt4Required { get; set; }

        // Checkboxes
        public bool CustomCheckbox1State { get; set; }
        public string? CustomCheckbox1Question { get; set; }
        public string? CustomCheckbox1Description { get; set; }
        public bool CustomCheckbox1ShowInTable { get; set; }
        public bool CustomCheckbox1Required { get; set; }

        public bool CustomCheckbox2State { get; set; }
        public string? CustomCheckbox2Question { get; set; }
        public string? CustomCheckbox2Description { get; set; }
        public bool CustomCheckbox2ShowInTable { get; set; }
        public bool CustomCheckbox2Required { get; set; }

        public bool CustomCheckbox3State { get; set; }
        public string? CustomCheckbox3Question { get; set; }
        public string? CustomCheckbox3Description { get; set; }
        public bool CustomCheckbox3ShowInTable { get; set; }
        public bool CustomCheckbox3Required { get; set; }

        public bool CustomCheckbox4State { get; set; }
        public string? CustomCheckbox4Question { get; set; }
        public string? CustomCheckbox4Description { get; set; }
        public bool CustomCheckbox4ShowInTable { get; set; }
        public bool CustomCheckbox4Required { get; set; }
    }
}