using FormBuilder.Core.Entities;

namespace FormBuilder.Core.Interfaces
{
    public interface IFormRepository : IRepository<Form>
    {
        Task<IEnumerable<Form>> GetUserFormsAsync(string userId);
        Task<Form?> GetUserFormForTemplateAsync(string userId, int templateId);
        Task<IEnumerable<Form>> GetTemplateFormsAsync(int templateId);
    }
}
