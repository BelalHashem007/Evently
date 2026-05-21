using EventBookingSystem.Data;
using EventBookingSystem.Models;
using EventBookingSystem.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EventBookingSystem.Repositories
{
    public class BookingRepository(AppDbContext context) : BaseRepository<Booking>(context), IBookingRepository
    {
        public async Task<Booking?> GetBookingWithBookingItemsById(int id, CancellationToken ct)
        {
            return await context.Bookings.Include(b => b.BookingItems).SingleOrDefaultAsync(b => b.Id == id, ct);
        }
    }
}
