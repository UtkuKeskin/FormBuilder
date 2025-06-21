namespace FormBuilder.Core.Entities
{
    public class Topic
    {
        public int Id { get; set; }
        public string Name { get; set; }
        
        // Navigation property
        public virtual ICollection<Template> Templates { get; set; }
    }
}
