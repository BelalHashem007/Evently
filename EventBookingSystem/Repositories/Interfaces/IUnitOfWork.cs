using EventBookingSystem.Models;

namespace EventBookingSystem.Repositories.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        public IBaseRepository<Event> Events { get; }
        public IBaseRepository<TicketType> TicketTypes { get; }
        public Task<int> CompleteAsync(CancellationToken ct = default);
    }
}
