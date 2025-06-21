using FormBuilder.Core.Entities;

namespace FormBuilder.Core.Interfaces
{
    public interface ITagRepository : IRepository<Tag>
    {
        Task<Tag?> GetByNameAsync(string name);
        Task<IEnumerable<Tag>> GetPopularTagsAsync(int count);
    }
}
