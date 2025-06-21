namespace FormBuilder.Core.Entities
{
    public class TemplateTag
    {
        public int TemplateId { get; set; }
        public int TagId { get; set; }
        
        public virtual Template Template { get; set; }
        public virtual Tag Tag { get; set; }
    }
}
