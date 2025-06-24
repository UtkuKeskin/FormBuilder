using Microsoft.EntityFrameworkCore;
using FormBuilder.Core.Entities;
using FormBuilder.Core.Interfaces;
using FormBuilder.Infrastructure.Data;

namespace FormBuilder.Infrastructure.Repositories
{
    public class FormRepository : Repository<Form>, IFormRepository
    {
        public FormRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<Form>> GetUserFormsAsync(string userId)
        {
            return await _context.Forms
                .Where(f => f.UserId == userId)
                .Include(f => f.Template)
                .OrderByDescending(f => f.FilledAt)
                .ToListAsync();
        }

        public async Task<Form?> GetUserFormForTemplateAsync(string userId, int templateId)
        {
            return await _context.Forms
                .Where(f => f.UserId == userId && f.TemplateId == templateId)
                .OrderByDescending(f => f.FilledAt)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Form>> GetTemplateFormsAsync(int templateId)
        {
            return await _context.Forms
                .Where(f => f.TemplateId == templateId)
                .Include(f => f.User)
                .OrderByDescending(f => f.FilledAt)
                .ToListAsync();
        }
    }
}