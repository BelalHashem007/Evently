using EventBookingSystem.Models;

namespace EventBookingSystem.Repositories.Interfaces
{
    public interface IBookingRepository : IBaseRepository<Booking>
    {
        public Task<Booking?> GetBookingWithBookingItemsById(int id, CancellationToken ct);
    }
}
