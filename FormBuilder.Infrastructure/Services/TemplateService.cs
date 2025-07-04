using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FormBuilder.Core.Entities;
using FormBuilder.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using FormBuilder.Infrastructure.Data;

namespace FormBuilder.Infrastructure.Services
{
    public class TemplateService : ITemplateService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _context;

        public TemplateService(IUnitOfWork unitOfWork, ApplicationDbContext context)
        {
            _unitOfWork = unitOfWork;
            _context = context;
        }

        public async Task<(IEnumerable<Template> templates, int totalCount)> GetTemplatesAsync(
            string userId = null,
            int page = 1,
            int pageSize = 10,
            string sortBy = "CreatedAt",
            string sortOrder = "desc",
            string searchTerm = null,
            int? topicId = null)
        {
            var query = _unitOfWork.Templates.GetQueryable();

            if (!string.IsNullOrEmpty(userId))
            {
                // MyTemplates
                query = query.Where(t => t.UserId == userId);
            }
            else
            {
                // Templates (Index) 
                query = query.Where(t => t.IsPublic);
            }

            // Filter by topic
            if (topicId.HasValue)
            {
                query = query.Where(t => t.TopicId == topicId.Value);
            }

            // Search
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(t => 
                    t.Title.Contains(searchTerm) || 
                    t.Description.Contains(searchTerm));
            }

            // Count before pagination
            var totalCount = await query.CountAsync();

            // Sorting
            query = ApplySorting(query, sortBy, sortOrder);

            // Include related data
            query = query
                .Include(t => t.User)
                .Include(t => t.Topic)
                .Include(t => t.Forms)
                    .ThenInclude(f => f.User)
                .Include(t => t.Likes)
                .Include(t => t.TemplateTags)
                    .ThenInclude(tt => tt.Tag);

            // Pagination
            var templates = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (templates, totalCount);
        }

        public async Task<Template> GetTemplateByIdAsync(int id)
        {
            var query = _unitOfWork.Templates.GetQueryable()
                .Include(t => t.User)
                .Include(t => t.Topic)
                .Include(t => t.Forms)
                    .ThenInclude(f => f.User)
                .Include(t => t.Likes)
                .Include(t => t.Comments)
                    .ThenInclude(c => c.User)
                .Include(t => t.TemplateTags)
                    .ThenInclude(tt => tt.Tag)
                .Include(t => t.TemplateAccesses)
                    .ThenInclude(ta => ta.User);

            return await query.FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<Template> CreateTemplateAsync(
            Template template, 
            string tags, 
            List<int> allowedUserIds)
        {
            // Add template
            await _unitOfWork.Templates.AddAsync(template);
            await _unitOfWork.SaveAsync();

            // Handle tags
            if (!string.IsNullOrEmpty(tags))
            {
                await ProcessTagsAsync(template, tags);
            }

            // Handle access control
            if (!template.IsPublic && allowedUserIds?.Any() == true)
            {
                await ProcessAccessControlAsync(template, allowedUserIds);
            }

            await _unitOfWork.SaveAsync();
            return template;
        }

        public async Task<Template> UpdateTemplateAsync(
            Template template, 
            string tags, 
            List<int> allowedUserIds)
        {
            // update logic...
            _unitOfWork.Templates.Update(template);

            // Update tags
            await UpdateTagsAsync(template, tags);

            // Update access control
            await UpdateAccessControlAsync(template, allowedUserIds);

            await _unitOfWork.SaveAsync();
            
            // EF Core change tracking
            _context.ChangeTracker.Clear();
            
            // reload template
            var updatedTemplate = await GetTemplateByIdAsync(template.Id);
            
            return updatedTemplate;
        }

        public async Task<bool> DeleteTemplateAsync(int id, string userId, bool isAdmin)
        {
            var template = await GetTemplateByIdAsync(id);
            if (template == null) return false;

            // Check permissions
            if (!isAdmin && template.UserId != userId) return false;

            _unitOfWork.Templates.Delete(template);
            await _unitOfWork.SaveAsync();
            return true;
        }

        public async Task<bool> CanUserAccessTemplateAsync(int templateId, string userId)
        {
            var template = await GetTemplateByIdAsync(templateId);
            if (template == null) return false;

            // Public templates are accessible to all
            if (template.IsPublic) return true;

            // Owner always has access
            if (template.UserId == userId) return true;

            // Check if user is in allowed list
            return template.TemplateAccesses.Any(ta => ta.UserId == userId);
        }

        public async Task<bool> CanUserEditTemplateAsync(int templateId, string userId, bool isAdmin)
        {
            var template = await GetTemplateByIdAsync(templateId);
            if (template == null) return false;

            // Admin can edit anything
            if (isAdmin) return true;

            // Only owner can edit
            return template.UserId == userId;
        }

        public async Task<List<Topic>> GetTopicsAsync()
        {
            return await _unitOfWork.Topics.GetAll().ToListAsync();
        }

        //Template Statistics & Aggregation Methods
        public async Task<Dictionary<string, object>> GetTemplateStatisticsAsync(int templateId)
        {
            var template = await GetTemplateByIdAsync(templateId);
            if (template == null) return new Dictionary<string, object>();
            
            var stats = new Dictionary<string, object>();
            var forms = template.Forms.ToList();
            
            AddBasicStats(stats, forms);
            AddIntegerQuestionStats(stats, template, forms);
            AddCheckboxQuestionStats(stats, template, forms);
            
            return stats;
        }

        // Small helper methods
        private void AddBasicStats(Dictionary<string, object> stats, List<Form> forms)
        {
            stats["totalResponses"] = forms.Count;
            stats["uniqueUsers"] = forms.Select(f => f.UserId).Distinct().Count();
            stats["todayResponses"] = forms.Count(f => f.FilledAt.Date == DateTime.Today);
            stats["averagePerDay"] = CalculateAveragePerDay(forms);
        }

        private void AddIntegerQuestionStats(
            Dictionary<string, object> stats, 
            Template template, 
            List<Form> forms)
        {
            for (int i = 1; i <= 4; i++)
            {
                if (IsQuestionActive(template, "Int", i))
                {
                    var answers = GetIntegerAnswers(forms, i);
                    if (answers.Any())
                    {
                        stats[$"int{i}_avg"] = answers.Average();
                        stats[$"int{i}_min"] = answers.Min();
                        stats[$"int{i}_max"] = answers.Max();
                    }
                }
            }
        }

        private void AddCheckboxQuestionStats(
            Dictionary<string, object> stats, 
            Template template, 
            List<Form> forms)
        {
            for (int i = 1; i <= 4; i++)
            {
                if (IsQuestionActive(template, "Checkbox", i))
                {
                    var trueCount = GetCheckboxTrueCount(forms, i);
                    stats[$"checkbox{i}_yes"] = trueCount;
                    stats[$"checkbox{i}_no"] = forms.Count - trueCount;
                    stats[$"checkbox{i}_percent"] = forms.Count > 0 ? 
                        Math.Round((double)trueCount / forms.Count * 100, 1) : 0;
                }
            }
        }

        private bool IsQuestionActive(Template template, string type, int number)
        {
            var prop = template.GetType().GetProperty($"Custom{type}{number}State");
            return prop?.GetValue(template) as bool? ?? false;
        }

        private List<int> GetIntegerAnswers(List<Form> forms, int number)
        {
            return forms
                .Select(f => f.GetType()
                    .GetProperty($"CustomInt{number}Answer")?
                    .GetValue(f) as int?)
                .Where(a => a.HasValue)
                .Select(a => a.Value)
                .ToList();
        }

        private int GetCheckboxTrueCount(List<Form> forms, int number)
        {
            return forms.Count(f => 
            {
                var value = f.GetType()
                    .GetProperty($"CustomCheckbox{number}Answer")?
                    .GetValue(f) as bool?;
                return value == true;
            });
        }

        private double CalculateAveragePerDay(List<Form> forms)
        {
            if (!forms.Any()) return 0;
            
            var firstDate = forms.Min(f => f.FilledAt).Date;
            var lastDate = forms.Max(f => f.FilledAt).Date;
            var daysDiff = (lastDate - firstDate).Days + 1;
            
            return Math.Round((double)forms.Count / daysDiff, 2);
        }

        // Private helper methods (existing)
        private IQueryable<Template> ApplySorting(
            IQueryable<Template> query, 
            string sortBy, 
            string sortOrder)
        {
            var isDescending = sortOrder?.ToLower() == "desc";

            return sortBy?.ToLower() switch
            {
                "title" => isDescending ? 
                    query.OrderByDescending(t => t.Title) : 
                    query.OrderBy(t => t.Title),
                "forms" => isDescending ? 
                    query.OrderByDescending(t => t.Forms.Count) : 
                    query.OrderBy(t => t.Forms.Count),
                "likes" => isDescending ? 
                    query.OrderByDescending(t => t.Likes.Count) : 
                    query.OrderBy(t => t.Likes.Count),
                _ => isDescending ? 
                    query.OrderByDescending(t => t.CreatedAt) : 
                    query.OrderBy(t => t.CreatedAt)
            };
        }

        private async Task ProcessTagsAsync(Template template, string tagString)
        {
            var tagNames = tagString
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(t => t.Trim().ToLower())
                .Where(t => !string.IsNullOrEmpty(t))
                .Distinct()
                .ToList();

            foreach (var tagName in tagNames)
            {
                var tag = await GetOrCreateTagAsync(tagName);
                template.TemplateTags.Add(new TemplateTag 
                { 
                    TemplateId = template.Id, 
                    TagId = tag.Id 
                });
            }
        }

        private async Task<Tag> GetOrCreateTagAsync(string tagName)
        {
            var existingTag = await _unitOfWork.Tags
                .GetQueryable()
                .FirstOrDefaultAsync(t => t.Name == tagName);

            if (existingTag != null)
                return existingTag;

            var newTag = new Tag { Name = tagName };
            await _unitOfWork.Tags.AddAsync(newTag);
            await _unitOfWork.SaveAsync();
            return newTag;
        }

        private async Task UpdateTagsAsync(Template template, string tagString)
        {
            // Remove existing tags
            var existingTags = await _unitOfWork.TemplateTags
                .GetQueryable()
                .Where(tt => tt.TemplateId == template.Id)
                .ToListAsync();

            foreach (var tag in existingTags)
            {
                _unitOfWork.TemplateTags.Delete(tag);
            }

            // Add new tags
            if (!string.IsNullOrEmpty(tagString))
            {
                await ProcessTagsAsync(template, tagString);
            }
        }

        private async Task ProcessAccessControlAsync(
            Template template, 
            List<int> allowedUserIds)
        {
            foreach (var userId in allowedUserIds)
            {
                var userExists = await _context.Users
                    .AnyAsync(u => u.Id == userId.ToString());

                if (userExists)
                {
                    template.TemplateAccesses.Add(new TemplateAccess
                    {
                        TemplateId = template.Id,
                        UserId = userId.ToString()
                    });
                }
            }
        }

        private async Task UpdateAccessControlAsync(
            Template template, 
            List<int> allowedUserIds)
        {
            // Remove existing access
            var existingAccess = await _unitOfWork.TemplateAccesses
                .GetQueryable()
                .Where(ta => ta.TemplateId == template.Id)
                .ToListAsync();

            foreach (var access in existingAccess)
            {
                _unitOfWork.TemplateAccesses.Delete(access);
            }

            // Add new access
            if (!template.IsPublic && allowedUserIds?.Any() == true)
            {
                await ProcessAccessControlAsync(template, allowedUserIds);
            }
        }
    }
}