using System.Collections.Generic;
using System.Threading.Tasks;
using FormBuilder.Core.Entities;

namespace FormBuilder.Core.Interfaces
{
    public interface ITemplateService
    {
        Task<(IEnumerable<Template> templates, int totalCount)> GetTemplatesAsync(
            string userId = null, 
            int page = 1, 
            int pageSize = 10,
            string sortBy = "CreatedAt",
            string sortOrder = "desc",
            string searchTerm = null,
            int? topicId = null);
            
        Task<Template> GetTemplateByIdAsync(int id);
        Task<Template> CreateTemplateAsync(Template template, string tags, List<int> allowedUserIds);
        Task<Template> UpdateTemplateAsync(Template template, string tags, List<int> allowedUserIds);
        Task<bool> DeleteTemplateAsync(int id, string userId, bool isAdmin);
        Task<bool> CanUserAccessTemplateAsync(int templateId, string userId);
        Task<bool> CanUserEditTemplateAsync(int templateId, string userId, bool isAdmin);
        Task<List<Topic>> GetTopicsAsync();
    }
}