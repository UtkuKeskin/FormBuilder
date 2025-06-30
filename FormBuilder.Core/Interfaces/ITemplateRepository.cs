using FormBuilder.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FormBuilder.Core.Interfaces
{
    public interface ITemplateRepository : IRepository<Template>
    {
        Task<IEnumerable<Template>> GetPublicTemplatesAsync();
        Task<IEnumerable<Template>> GetUserTemplatesAsync(string userId);
        Task<IEnumerable<Template>> GetLatestTemplatesAsync(int count);
        Task<IEnumerable<Template>> GetPopularTemplatesAsync(int count);
        void Delete(Template template);
    }
}