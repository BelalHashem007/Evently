using System.Linq.Expressions;

namespace EventBookingSystem.Repositories.Interfaces
{
    public interface IBaseRepository<T> where T : class
    {
        public Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default);
        public Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
        public Task<T?> FindOneAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
        public Task<T?> GetByIdAsync(int id, CancellationToken ct = default);
        public Task<T> AddAsync(T entity, CancellationToken ct = default);
        public T Update(T entity);
        public Task<bool> DeleteAsync(int id, CancellationToken ct = default);
    }
}
