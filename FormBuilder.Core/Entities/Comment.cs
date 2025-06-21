namespace FormBuilder.Core.Entities
{
    public class Comment : BaseEntity
    {
        public int TemplateId { get; set; }
        public string UserId { get; set; }
        public string Text { get; set; }
        
        // Navigation properties
        public virtual Template Template { get; set; }
        public virtual User User { get; set; }
    }
}
