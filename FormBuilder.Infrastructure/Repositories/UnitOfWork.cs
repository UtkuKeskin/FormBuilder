using FormBuilder.Core.Entities;
using FormBuilder.Core.Interfaces;
using FormBuilder.Infrastructure.Data;

namespace FormBuilder.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private ITemplateRepository? _templates;
        private IFormRepository? _forms;
        private ITagRepository? _tags;
        private IRepository<Topic>? _topics;
        private IRepository<Comment>? _comments;
        private IRepository<Like>? _likes;
        private IRepository<TemplateTag>? _templateTags;
        private IRepository<TemplateAccess>? _templateAccesses;
        private IRepository<User>? _users;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        public ITemplateRepository Templates => 
            _templates ??= new TemplateRepository(_context);

        public IFormRepository Forms => 
            _forms ??= new FormRepository(_context);

        public ITagRepository Tags => 
            _tags ??= new TagRepository(_context);

        public IRepository<Topic> Topics => 
            _topics ??= new Repository<Topic>(_context);

        public IRepository<Comment> Comments => 
            _comments ??= new Repository<Comment>(_context);

        public IRepository<Like> Likes => 
            _likes ??= new Repository<Like>(_context);

        public IRepository<TemplateTag> TemplateTags => 
            _templateTags ??= new Repository<TemplateTag>(_context);

        public IRepository<TemplateAccess> TemplateAccesses => 
            _templateAccesses ??= new Repository<TemplateAccess>(_context);

        public IRepository<User> Users => 
            _users ??= new Repository<User>(_context);

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task<int> SaveAsync() // SaveAsync implementation
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}