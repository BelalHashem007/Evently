using EventBookingSystem.Data;
using EventBookingSystem.Models;
using EventBookingSystem.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EventBookingSystem.Repositories
{
    public class NotificationRepository : BaseRepository<Notification>, INotificationRepository
    {
        public NotificationRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IReadOnlyList<Notification>> GetLatestForUserAsync(int userId, int skip, int take, CancellationToken ct = default)
        {
            return await context.Set<Notification>()
                .AsNoTracking()
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedDate)
                .Skip(skip)
                .Take(take + 1)
                .ToListAsync(ct);
        }

        public async Task UpdateReadStatus(int userId, CancellationToken ct = default)
        {
            await context.Set<Notification>()
                .Where(n => n.UserId == userId && !n.IsRead)
                .ExecuteUpdateAsync(setters => setters.SetProperty(n => n.IsRead, true), ct);
        }
    }
}
