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
        public Task<IReadOnlyList<T>> FindWithPaginationAsync(Expression<Func<T, bool>> predicate, int skip = 0, int take = 15, CancellationToken ct = default);
        public Task<IReadOnlyList<T>> FindWithPaginationAsync<TKey>(Expression<Func<T, bool>> predicate, Expression<Func<T, TKey>> order , int skip = 0, int take = 15, bool isDescending = false, CancellationToken ct = default);
        public Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken ct = default);
    }
}
