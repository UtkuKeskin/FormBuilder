using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FormBuilder.Core.Entities;
using FormBuilder.Core.Interfaces;
using FormBuilder.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FormBuilder.Infrastructure.Services
{
    public class TagService : ITagService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TagService> _logger;

        public TagService(
            IUnitOfWork unitOfWork, 
            ApplicationDbContext context,
            ILogger<TagService> logger)
        {
            _unitOfWork = unitOfWork;
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<Tag>> GetAllTagsAsync()
        {
            return await _unitOfWork.Tags.GetAllAsync();
        }

        public async Task<IEnumerable<Tag>> GetPopularTagsAsync(int count)
        {
            return await _unitOfWork.Tags.GetPopularTagsAsync(count);
        }

        public async Task<Tag> GetOrCreateTagAsync(string tagName)
        {
            // Normalize tag name
            tagName = NormalizeTagName(tagName);
            
            // Check if exists
            var existingTag = await _unitOfWork.Tags.GetByNameAsync(tagName);
            if (existingTag != null)
                return existingTag;

            // Create new
            var newTag = new Tag { Name = tagName };
            await _unitOfWork.Tags.AddAsync(newTag);
            await _unitOfWork.SaveAsync();
            
            return newTag;
        }

        public async Task<IEnumerable<string>> GetTagSuggestionsAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
                return Enumerable.Empty<string>();

            query = query.ToLower().Trim();

            var suggestions = await _context.Tags
                .Where(t => t.Name.ToLower().StartsWith(query))
                .OrderByDescending(t => t.TemplateTags.Count)
                .Take(10)
                .Select(t => t.Name)
                .ToListAsync();

            return suggestions;
        }

        public async Task<Dictionary<string, int>> GetTagCloudDataAsync(int maxTags = 30)
        {
            var tagData = await _context.Tags
                .Include(t => t.TemplateTags)
                .Where(t => t.TemplateTags.Any())
                .OrderByDescending(t => t.TemplateTags.Count)
                .Take(maxTags)
                .Select(t => new { t.Name, Count = t.TemplateTags.Count })
                .ToListAsync();

            return tagData.ToDictionary(t => t.Name, t => t.Count);
        }

        public async Task CleanupUnusedTagsAsync()
        {
            var unusedTags = await _context.Tags
                .Where(t => !t.TemplateTags.Any())
                .ToListAsync();

            if (unusedTags.Any())
            {
                _context.Tags.RemoveRange(unusedTags);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Cleaned up {Count} unused tags", unusedTags.Count);
            }
        }

        public async Task<int> GetTagUsageCountAsync(int tagId)
        {
            return await _context.TemplateTags
                .CountAsync(tt => tt.TagId == tagId);
        }

        // Helper method
        private string NormalizeTagName(string tagName)
        {
            return tagName?.Trim().ToLower() ?? string.Empty;
        }
    }
}