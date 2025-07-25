using System.Linq.Expressions;

namespace FormBuilder.Core.Interfaces
{
    public interface IRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(int id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
        Task AddAsync(T entity);
        void Update(T entity);
        void Remove(T entity);
        void Delete(T entity);
        IQueryable<T> GetQueryable();
        IQueryable<T> GetAll();
    }
}
