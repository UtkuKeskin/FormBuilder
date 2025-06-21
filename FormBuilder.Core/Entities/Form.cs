namespace FormBuilder.Core.Entities
{
    public class Form : BaseEntity
    {
        public int TemplateId { get; set; }
        public string UserId { get; set; }
        public DateTime FilledAt { get; set; }
        
        // String Answers (1-4)
        public string CustomString1Answer { get; set; }
        public string CustomString2Answer { get; set; }
        public string CustomString3Answer { get; set; }
        public string CustomString4Answer { get; set; }
        
        // Text Answers (1-4)
        public string CustomText1Answer { get; set; }
        public string CustomText2Answer { get; set; }
        public string CustomText3Answer { get; set; }
        public string CustomText4Answer { get; set; }
        
        // Integer Answers (1-4)
        public int? CustomInt1Answer { get; set; }
        public int? CustomInt2Answer { get; set; }
        public int? CustomInt3Answer { get; set; }
        public int? CustomInt4Answer { get; set; }
        
        // Checkbox Answers (1-4)
        public bool CustomCheckbox1Answer { get; set; }
        public bool CustomCheckbox2Answer { get; set; }
        public bool CustomCheckbox3Answer { get; set; }
        public bool CustomCheckbox4Answer { get; set; }
        
        // Navigation properties
        public virtual Template Template { get; set; }
        public virtual User User { get; set; }
    }
}
