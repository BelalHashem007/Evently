using EventBookingSystem.Areas.Admin.ViewModels;
using EventBookingSystem.Models;
using EventBookingSystem.Repositories.Interfaces;
using EventBookingSystem.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EventBookingSystem.Services
{
    public class BookingService(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager) : IBookingService
    {
        public async Task<IReadOnlyList<AdminBookingListItemViewModel>> GetAdminBookingListAsync(CancellationToken ct = default)
        {
            var bookings = (await unitOfWork.Bookings.GetAllAsync(ct))
                .OrderByDescending(b => b.CreatedDate)
                .ToList();

            if (bookings.Count == 0)
            {
                return [];
            }

            var eventIds = bookings.Select(b => b.EventId).Distinct().ToList();
            var userIds = bookings.Select(b => b.UserId).Distinct().ToList();
            var eventsById = (await unitOfWork.Events.FindAsync(e => eventIds.Contains(e.Id), ct))
                .ToDictionary(e => e.Id);
            var usersById = await userManager.Users
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, ct);

            return bookings
                .Select(b => new AdminBookingListItemViewModel
                {
                    Id = b.Id,
                    EventName = eventsById.TryGetValue(b.EventId, out var eventItem) ? eventItem.Name : "Unknown event",
                    UserName = usersById.TryGetValue(b.UserId, out var user) ? user.UserName ?? user.Email ?? "Unknown user" : "Unknown user",
                    TotalPrice = b.TotalPrice,
                    CreatedDate = b.CreatedDate
                })
                .ToList();
        }
    }
}
