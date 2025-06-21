namespace FormBuilder.Core.Entities
{
    public class Like
    {
        public int TemplateId { get; set; }
        public string UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        
        // Navigation properties
        public virtual Template Template { get; set; }
        public virtual User User { get; set; }
    }
}
