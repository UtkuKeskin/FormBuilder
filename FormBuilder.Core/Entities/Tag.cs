namespace FormBuilder.Core.Entities
{
    public class Tag
    {
        public int Id { get; set; }
        public string Name { get; set; }
        
        // Navigation property
        public virtual ICollection<TemplateTag> TemplateTags { get; set; }
    }
}
