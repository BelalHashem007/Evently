using System.Linq.Expressions;
using EventBookingSystem.Data;
using EventBookingSystem.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EventBookingSystem.Repositories
{
    public class BaseRepository<T> : IBaseRepository<T> where T : class
    {
        protected AppDbContext context;

        public BaseRepository(AppDbContext dbContext)
        {
            context = dbContext;
        }
        public async Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default)
        {
            return await context.Set<T>().ToListAsync<T>(ct);
        }

        public async Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
        {
            return await context.Set<T>().Where(predicate).ToListAsync(ct);
        }

        public async Task<T?> FindOneAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
        {
            return await context.Set<T>().SingleOrDefaultAsync(predicate, ct);
        }

        public async Task<T?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await context.Set<T>().FindAsync(id, ct);
        }

        public async Task<T> AddAsync(T entity, CancellationToken ct = default)
        {
            await context.AddAsync(entity, ct);
            return entity;
        }

        public T Update(T entity)
        {
            context.Set<T>().Update(entity);
            return entity;
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        {
            var entity = await context.Set<T>().FindAsync(id, ct);
            if (entity == null)
                return false;

            context.Set<T>().Remove(entity);
            return true;
        }

        public async Task<IReadOnlyList<T>> FindWithPaginationAsync(
             Expression<Func<T, bool>> predicate,
             int skip = 0,
             int take = 15,
             CancellationToken ct = default)
        {
            var query = context.Set<T>().Where(predicate);

            query = query.Skip(skip).Take(take);

            return await query.ToListAsync(ct);
        }

        public async Task<IReadOnlyList<T>> FindWithPaginationAsync<TKey>(
            Expression<Func<T, bool>> predicate, 
            Expression<Func<T, TKey>> order, 
            int skip = 0, 
            int take = 15,
            bool isDescending = false,
            CancellationToken ct = default)
        {
            var query = context.Set<T>().Where(predicate);
            if (isDescending)
                query = query.OrderByDescending(order).Skip(skip).Take(take);
            else query = query.OrderBy(order).Skip(skip).Take(take);
            return await query.ToListAsync(ct);
        }

        public async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken ct = default)
        {
            if (predicate == null)
                return await context.Set<T>().CountAsync(ct);
            else return await context.Set<T>().Where(predicate).CountAsync(ct);
        }
    }
}
