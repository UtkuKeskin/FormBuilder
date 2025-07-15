using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using FormBuilder.Core.Entities;
using FormBuilder.Core.Interfaces;
using FormBuilder.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FormBuilder.Infrastructure.Services
{
    public class ApiKeyService : IApiKeyService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ApiKeyService> _logger;
        private const string API_KEY_PREFIX = "FB_";
        private const int API_KEY_LENGTH = 40;

        public ApiKeyService(ApplicationDbContext context, ILogger<ApiKeyService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<string> GenerateApiKeyAsync(string userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                throw new ArgumentException("User not found");
            }

            // Generate unique API key
            string apiKey;
            do
            {
                apiKey = GenerateUniqueApiKey();
            } while (await _context.Users.AnyAsync(u => u.ApiKey == apiKey));

            // Update user
            user.ApiKey = apiKey;
            user.ApiKeyGeneratedAt = DateTime.UtcNow;
            user.ApiKeyEnabled = true;
            user.ApiKeyLastUsedAt = null;

            await _context.SaveChangesAsync();
            _logger.LogInformation("API key generated for user {UserId}", userId);

            return apiKey;
        }

        public async Task<bool> RevokeApiKeyAsync(string userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            user.ApiKey = null;
            user.ApiKeyGeneratedAt = null;
            user.ApiKeyEnabled = false;
            user.ApiKeyLastUsedAt = null;

            await _context.SaveChangesAsync();
            _logger.LogInformation("API key revoked for user {UserId}", userId);

            return true;
        }

        public async Task<bool> ValidateApiKeyAsync(string apiKey)
        {
            if (string.IsNullOrEmpty(apiKey) || !apiKey.StartsWith(API_KEY_PREFIX))
                return false;

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.ApiKey == apiKey && u.ApiKeyEnabled);

            return user != null;
        }

        public async Task<User> GetUserByApiKeyAsync(string apiKey)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.ApiKey == apiKey && u.ApiKeyEnabled);
        }

        public async Task UpdateLastUsedAsync(string apiKey)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.ApiKey == apiKey);

            if (user != null)
            {
                user.ApiKeyLastUsedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        private string GenerateUniqueApiKey()
        {
            // Combine GUID with timestamp hash
            var guid = Guid.NewGuid().ToString("N");
            var timestamp = DateTime.UtcNow.Ticks.ToString();
            var combined = $"{guid}{timestamp}";

            using (var sha256 = SHA256.Create())
            {
                var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(combined));
                var hashString = Convert.ToBase64String(hashBytes)
                    .Replace("+", "")
                    .Replace("/", "")
                    .Replace("=", "");

                // Ensure exact length
                var key = $"{API_KEY_PREFIX}{hashString}";
                return key.Length > API_KEY_LENGTH 
                    ? key.Substring(0, API_KEY_LENGTH) 
                    : key.PadRight(API_KEY_LENGTH, '0');
            }
        }
    }
}