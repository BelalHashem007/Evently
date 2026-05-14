using EventBookingSystem.Data;
using EventBookingSystem.Models;
using EventBookingSystem.Repositories.Interfaces;

namespace EventBookingSystem.Repositories
{
    public class UnitOfWork(
        IBaseRepository<Event> eventRepo,
        IBaseRepository<TicketType> ticketTypeRepo,
        AppDbContext context) : IUnitOfWork
    {
        public IBaseRepository<Event> Events => eventRepo;
        public IBaseRepository<TicketType> TicketTypes => ticketTypeRepo;

        public async Task<int> CompleteAsync(CancellationToken ct)
        {
            return await context.SaveChangesAsync(ct);
        }

        public void Dispose()
        {
            context.Dispose();
        }
    }
}
