using FormBuilder.Core.Entities;

namespace FormBuilder.Core.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        ITemplateRepository Templates { get; }
        IFormRepository Forms { get; }
        ITagRepository Tags { get; }
        IRepository<Topic> Topics { get; }
        IRepository<Comment> Comments { get; }
        IRepository<Like> Likes { get; }

        IRepository<TemplateTag> TemplateTags { get; }
        IRepository<TemplateAccess> TemplateAccesses { get; }
        

        Task<int> SaveChangesAsync();
        Task<int> SaveAsync();
    }
}
