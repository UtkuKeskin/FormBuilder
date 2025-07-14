using Microsoft.AspNetCore.Identity;

namespace FormBuilder.Core.Entities
{
    public class User : IdentityUser
    {
        public bool IsAdmin { get; set; }
        public string Theme { get; set; } = "light";
        public string Language { get; set; } = "en";
        public DateTime? LastLoginAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public int Version { get; set; }

        // Navigation properties
        public virtual ICollection<Template> Templates { get; set; }
        public virtual ICollection<Form> Forms { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }
        public virtual ICollection<Like> Likes { get; set; }
        
        // Salesforce Integration Fields
        public string? SalesforceAccountId { get; set; }
        public string? SalesforceContactId { get; set; }
        public DateTime? LastSalesforceSync { get; set; }
        public bool SalesforceIntegrationEnabled { get; set; } = false;
    }
}
