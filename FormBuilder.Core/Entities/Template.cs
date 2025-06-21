namespace FormBuilder.Core.Entities
{
    public class Template : BaseEntity
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public string UserId { get; set; }
        public int TopicId { get; set; }
        public bool IsPublic { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // String Questions
        public bool CustomString1State { get; set; }
        public string CustomString1Question { get; set; }
        public string CustomString1Description { get; set; }
        public bool CustomString1ShowInTable { get; set; }
        
        public bool CustomString2State { get; set; }
        public string CustomString2Question { get; set; }
        public string CustomString2Description { get; set; }
        public bool CustomString2ShowInTable { get; set; }
        
        public bool CustomString3State { get; set; }
        public string CustomString3Question { get; set; }
        public string CustomString3Description { get; set; }
        public bool CustomString3ShowInTable { get; set; }
        
        public bool CustomString4State { get; set; }
        public string CustomString4Question { get; set; }
        public string CustomString4Description { get; set; }
        public bool CustomString4ShowInTable { get; set; }
        
        // Text Questions (1-4)
        public bool CustomText1State { get; set; }
        public string CustomText1Question { get; set; }
        public string CustomText1Description { get; set; }
        public bool CustomText1ShowInTable { get; set; }
        
        public bool CustomText2State { get; set; }
        public string CustomText2Question { get; set; }
        public string CustomText2Description { get; set; }
        public bool CustomText2ShowInTable { get; set; }
        
        public bool CustomText3State { get; set; }
        public string CustomText3Question { get; set; }
        public string CustomText3Description { get; set; }
        public bool CustomText3ShowInTable { get; set; }
        
        public bool CustomText4State { get; set; }
        public string CustomText4Question { get; set; }
        public string CustomText4Description { get; set; }
        public bool CustomText4ShowInTable { get; set; }
        
        // Integer Questions (1-4)
        public bool CustomInt1State { get; set; }
        public string CustomInt1Question { get; set; }
        public string CustomInt1Description { get; set; }
        public bool CustomInt1ShowInTable { get; set; }
        
        public bool CustomInt2State { get; set; }
        public string CustomInt2Question { get; set; }
        public string CustomInt2Description { get; set; }
        public bool CustomInt2ShowInTable { get; set; }
        
        public bool CustomInt3State { get; set; }
        public string CustomInt3Question { get; set; }
        public string CustomInt3Description { get; set; }
        public bool CustomInt3ShowInTable { get; set; }
        
        public bool CustomInt4State { get; set; }
        public string CustomInt4Question { get; set; }
        public string CustomInt4Description { get; set; }
        public bool CustomInt4ShowInTable { get; set; }
        
        // Checkbox Questions (1-4)
        public bool CustomCheckbox1State { get; set; }
        public string CustomCheckbox1Question { get; set; }
        public string CustomCheckbox1Description { get; set; }
        public bool CustomCheckbox1ShowInTable { get; set; }
        
        public bool CustomCheckbox2State { get; set; }
        public string CustomCheckbox2Question { get; set; }
        public string CustomCheckbox2Description { get; set; }
        public bool CustomCheckbox2ShowInTable { get; set; }
        
        public bool CustomCheckbox3State { get; set; }
        public string CustomCheckbox3Question { get; set; }
        public string CustomCheckbox3Description { get; set; }
        public bool CustomCheckbox3ShowInTable { get; set; }
        
        public bool CustomCheckbox4State { get; set; }
        public string CustomCheckbox4Question { get; set; }
        public string CustomCheckbox4Description { get; set; }
        public bool CustomCheckbox4ShowInTable { get; set; }
        
        // Navigation properties
        public virtual User User { get; set; }
        public virtual Topic Topic { get; set; }
        public virtual ICollection<Form> Forms { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }
        public virtual ICollection<Like> Likes { get; set; }
        public virtual ICollection<TemplateTag> TemplateTags { get; set; }
        public virtual ICollection<TemplateAccess> TemplateAccesses { get; set; }
    }
}
