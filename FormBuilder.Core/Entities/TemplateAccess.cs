namespace FormBuilder.Core.Entities
{
    public class TemplateAccess
    {
        public int TemplateId { get; set; }
        public string UserId { get; set; }
        
        // Navigation properties
        public virtual Template Template { get; set; }
        public virtual User User { get; set; }
    }
}
