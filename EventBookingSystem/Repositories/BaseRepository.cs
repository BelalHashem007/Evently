using System.Linq.Expressions;
using EventBookingSystem.Data;
using EventBookingSystem.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EventBookingSystem.Repositories
{
    public class BaseRepository<T>(AppDbContext context) : IBaseRepository<T> where T : class
    {
        public async Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default)
        {
            return await context.Set<T>().ToListAsync<T>(ct);
        }

        public async Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
        {
            return await context.Set<T>().Where(predicate).ToListAsync(ct);
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

    }
}
