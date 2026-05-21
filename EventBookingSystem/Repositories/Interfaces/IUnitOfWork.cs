using EventBookingSystem.Common.Results;
using EventBookingSystem.Models;

namespace EventBookingSystem.Repositories.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        public IBaseRepository<Event> Events { get; }
        public IBaseRepository<TicketType> TicketTypes { get; }
        public IBookingRepository Bookings { get; }
        public IBaseRepository<BookingItem> BookingItems { get; }
        public IBaseRepository<Payment> Payments { get; }
        public Task<int> CompleteAsync(CancellationToken ct = default);
        public Task<Result> TryCompeleteAsync(CancellationToken ct);
    }
}
