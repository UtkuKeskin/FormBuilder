using System.Threading.Tasks;
using FormBuilder.Core.Entities;

namespace FormBuilder.Core.Interfaces
{
    public interface IApiKeyService
    {
        Task<string> GenerateApiKeyAsync(string userId);
        Task<bool> RevokeApiKeyAsync(string userId);
        Task<bool> ValidateApiKeyAsync(string apiKey);
        Task<User> GetUserByApiKeyAsync(string apiKey);
        Task UpdateLastUsedAsync(string apiKey);
    }
}