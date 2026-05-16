using EventBookingSystem.Common.Results;
using EventBookingSystem.Data;
using EventBookingSystem.Models;
using EventBookingSystem.Repositories.Interfaces;

namespace EventBookingSystem.Repositories
{
    public class UnitOfWork(
        IBaseRepository<Event> eventRepo,
        IBaseRepository<TicketType> ticketTypeRepo,
        IBaseRepository<Booking> bookingRepo,
        IBaseRepository<BookingItem> bookingItemRepo,
        AppDbContext context,
        ILogger<UnitOfWork> logger
        ) : IUnitOfWork
    {
        public IBaseRepository<Event> Events => eventRepo;
        public IBaseRepository<TicketType> TicketTypes => ticketTypeRepo;
        public IBaseRepository<Booking> Bookings => bookingRepo;
        public IBaseRepository<BookingItem> BookingItems => bookingItemRepo;

        public async Task<int> CompleteAsync(CancellationToken ct)
        {
            return await context.SaveChangesAsync(ct);
        }

        public async Task<Result> TryCompeleteAsync(CancellationToken ct)
        {
            try
            {
                await context.SaveChangesAsync(ct);
                return Result.Success();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while saving to database.");
                return Result.Failure("An error occurred while saving to database.");
            }
        }
        public void Dispose()
        {
            context.Dispose();
        }
    }
}
