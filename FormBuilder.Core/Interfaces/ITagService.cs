using System.Collections.Generic;
using System.Threading.Tasks;
using FormBuilder.Core.Entities;

namespace FormBuilder.Core.Interfaces
{
    public interface ITagService
    {
        Task<IEnumerable<Tag>> GetAllTagsAsync();
        Task<IEnumerable<Tag>> GetPopularTagsAsync(int count);
        Task<Tag> GetOrCreateTagAsync(string tagName);
        Task<IEnumerable<string>> GetTagSuggestionsAsync(string query);
        Task<Dictionary<string, int>> GetTagCloudDataAsync(int maxTags = 30);
        Task CleanupUnusedTagsAsync();
        Task<int> GetTagUsageCountAsync(int tagId);
    }
}