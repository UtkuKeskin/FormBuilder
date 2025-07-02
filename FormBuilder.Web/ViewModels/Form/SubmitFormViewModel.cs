using System.ComponentModel.DataAnnotations;

namespace FormBuilder.Web.ViewModels.Form
{
    public class SubmitFormViewModel
    {
        [Required]
        public int TemplateId { get; set; }
        
        // String answers
        public string? CustomString1Answer { get; set; }
        public string? CustomString2Answer { get; set; }
        public string? CustomString3Answer { get; set; }
        public string? CustomString4Answer { get; set; }
        
        // Text answers
        public string? CustomText1Answer { get; set; }
        public string? CustomText2Answer { get; set; }
        public string? CustomText3Answer { get; set; }
        public string? CustomText4Answer { get; set; }
        
        // Integer answers
        public int? CustomInt1Answer { get; set; }
        public int? CustomInt2Answer { get; set; }
        public int? CustomInt3Answer { get; set; }
        public int? CustomInt4Answer { get; set; }
        
        // Checkbox answers
        public bool CustomCheckbox1Answer { get; set; }
        public bool CustomCheckbox2Answer { get; set; }
        public bool CustomCheckbox3Answer { get; set; }
        public bool CustomCheckbox4Answer { get; set; }
        
        // Optional
        public bool SendEmailCopy { get; set; }
    }
}