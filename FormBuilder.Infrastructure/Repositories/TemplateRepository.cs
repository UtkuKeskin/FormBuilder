using Microsoft.EntityFrameworkCore;
using FormBuilder.Core.Entities;
using FormBuilder.Core.Interfaces;
using FormBuilder.Infrastructure.Data;

namespace FormBuilder.Infrastructure.Repositories
{
    public class TemplateRepository : Repository<Template>, ITemplateRepository
    {
        public TemplateRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<Template>> GetPublicTemplatesAsync()
        {
            return await _context.Templates
                .Where(t => t.IsPublic)
                .Include(t => t.User)
                .Include(t => t.Topic)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Template>> GetUserTemplatesAsync(string userId)
        {
            return await _context.Templates
                .Where(t => t.UserId == userId)
                .Include(t => t.Topic)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Template>> GetLatestTemplatesAsync(int count)
        {
            return await _context.Templates
                .Where(t => t.IsPublic)
                .Include(t => t.User)
                .Include(t => t.Topic)
                .OrderByDescending(t => t.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<Template>> GetPopularTemplatesAsync(int count)
        {
            return await _context.Templates
                .Where(t => t.IsPublic)
                .Include(t => t.User)
                .Include(t => t.Topic)
                .Include(t => t.Forms)
                .OrderByDescending(t => t.Forms.Count)
                .Take(count)
                .ToListAsync();
        }

        public void Delete(Template template)
        {
            _context.Templates.Remove(template);
        }
    }
}