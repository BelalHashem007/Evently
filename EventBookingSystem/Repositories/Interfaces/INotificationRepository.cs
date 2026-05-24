using EventBookingSystem.Models;

namespace EventBookingSystem.Repositories.Interfaces
{
    public interface INotificationRepository : IBaseRepository<Notification>
    {
        public Task<IReadOnlyList<Notification>> GetLatestForUserAsync(int userId, int skip, int take, CancellationToken ct = default);
        public Task UpdateReadStatus(int userId, CancellationToken ct = default);
    }
}
