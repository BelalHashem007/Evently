using EventBookingSystem.Areas.Admin.ViewModels;
using EventBookingSystem.Common.Results;
using EventBookingSystem.Models;
using EventBookingSystem.Repositories.Interfaces;
using EventBookingSystem.Services.Interfaces;
using EventBookingSystem.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EventBookingSystem.Services
{
    public class BookingService(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager) : IBookingService
    {
        private static readonly TimeSpan PendingBookingLifetime = TimeSpan.FromMinutes(15);

        public async Task<IReadOnlyList<AdminBookingListItemViewModel>> GetAdminBookingListAsync(CancellationToken ct = default)
        {
            await ExpirePendingBookingsAsync(ct);

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
                    CreatedDate = b.CreatedDate,
                    Status = b.Status,
                    ExpiresAt = b.ExpiresAt
                })
                .ToList();
        }

        public async Task<Result<int>> CreateBookingAsync(CreateBookingViewModel model, int userId, CancellationToken ct = default)
        {
            await ExpirePendingBookingsAsync(ct);

            var eventItem = await unitOfWork.Events.GetByIdAsync(model.EventId, ct);
            if (eventItem == null)
            {
                return Result<int>.Failure("Event was not found.");
            }

            if (eventItem.IsCancelled)
            {
                return Result<int>.Failure("Cancelled events cannot be booked.");
            }

            var requestedTickets = model.Tickets
                .Where(t => t.Quantity > 0)
                .GroupBy(t => t.TicketTypeId)
                .ToDictionary(g => g.Key, g => g.Sum(t => t.Quantity));

            if (requestedTickets.Count == 0)
            {
                return Result<int>.Failure("Select at least one ticket.");
            }

            var ticketTypes = (await unitOfWork.TicketTypes.FindAsync(t => t.EventId == model.EventId, ct))
                .ToDictionary(t => t.Id);

            if (requestedTickets.Keys.Any(ticketTypeId => !ticketTypes.ContainsKey(ticketTypeId)))
            {
                return Result<int>.Failure("One or more selected ticket types are invalid.");
            }

            var bookedQuantities = await GetActiveBookedQuantitiesAsync(ticketTypes.Keys, model.EventId, ct);
            foreach (var (ticketTypeId, requestedQuantity) in requestedTickets)
            {
                var ticketType = ticketTypes[ticketTypeId];
                var availableQuantity = ticketType.Quantity - bookedQuantities.GetValueOrDefault(ticketTypeId);
                if (requestedQuantity > availableQuantity)
                {
                    return Result<int>.Failure($"Only {Math.Max(0, availableQuantity)} tickets are available for {ticketType.Name}.");
                }
            }

            var booking = new Booking
            {
                UserId = userId,
                EventId = model.EventId,
                CreatedDate = DateTime.UtcNow,
                Status = BookingStatus.Pending,
                ExpiresAt = DateTime.UtcNow.Add(PendingBookingLifetime)
            };

            foreach (var (ticketTypeId, quantity) in requestedTickets)
            {
                var ticketType = ticketTypes[ticketTypeId];
                var itemTotal = ticketType.Price * quantity;
                booking.TotalPrice += itemTotal;
                booking.BookingItems.Add(new BookingItem
                {
                    TicketTypeId = ticketTypeId,
                    Quantity = quantity,
                    TotalPrice = itemTotal
                });
            }

            await unitOfWork.Bookings.AddAsync(booking, ct);
            var dbResult = await unitOfWork.TryCompeleteAsync(ct);
            return dbResult.Succeeded
                ? Result<int>.Success(booking.Id)
                : Result<int>.Failure(dbResult.ErrorMessage ?? "Could not create booking.");
        }

        public async Task<IReadOnlyList<BookingListItemViewModel>> GetUserBookingsAsync(int userId, CancellationToken ct = default)
        {
            await ExpirePendingBookingsAsync(ct);

            var bookings = (await unitOfWork.Bookings.FindAsync(b => b.UserId == userId, ct))
                .OrderByDescending(b => b.CreatedDate)
                .ToList();

            if (bookings.Count == 0)
            {
                return [];
            }

            var eventIds = bookings.Select(b => b.EventId).Distinct().ToList();
            var eventsById = (await unitOfWork.Events.FindAsync(e => eventIds.Contains(e.Id), ct))
                .ToDictionary(e => e.Id);

            return bookings
                .Select(b => new BookingListItemViewModel
                {
                    Id = b.Id,
                    EventName = eventsById.TryGetValue(b.EventId, out var eventItem) ? eventItem.Name : "Unknown event",
                    EventDate = eventsById.TryGetValue(b.EventId, out eventItem) ? eventItem.Date : default,
                    Status = b.Status,
                    CreatedDate = b.CreatedDate,
                    ExpiresAt = b.ExpiresAt,
                    TotalPrice = b.TotalPrice
                })
                .ToList();
        }

        public async Task<BookingDetailsViewModel?> GetUserBookingDetailsAsync(int bookingId, int userId, CancellationToken ct = default)
        {
            await ExpirePendingBookingsAsync(ct);

            var booking = await unitOfWork.Bookings.GetByIdAsync(bookingId, ct);
            if (booking == null || booking.UserId != userId)
            {
                return null;
            }

            var eventItem = await unitOfWork.Events.GetByIdAsync(booking.EventId, ct);
            var bookingItems = await unitOfWork.BookingItems.FindAsync(i => i.BookingId == booking.Id, ct);
            var ticketTypeIds = bookingItems.Select(i => i.TicketTypeId).Distinct().ToList();
            var ticketTypesById = ticketTypeIds.Count == 0
                ? new Dictionary<int, TicketType>()
                : (await unitOfWork.TicketTypes.FindAsync(t => ticketTypeIds.Contains(t.Id), ct))
                    .ToDictionary(t => t.Id);

            return new BookingDetailsViewModel
            {
                Id = booking.Id,
                EventName = eventItem?.Name ?? "Unknown event",
                EventDate = eventItem?.Date ?? default,
                Venue = eventItem?.Venue ?? "Unknown venue",
                Status = booking.Status,
                CreatedDate = booking.CreatedDate,
                ExpiresAt = booking.ExpiresAt,
                TotalPrice = booking.TotalPrice,
                Items = bookingItems
                    .Select(i =>
                    {
                        ticketTypesById.TryGetValue(i.TicketTypeId, out var ticketType);
                        return new BookingItemDetailsViewModel
                        {
                            TicketTypeName = ticketType?.Name ?? "Unknown ticket",
                            UnitPrice = i.Quantity == 0 ? 0 : i.TotalPrice / i.Quantity,
                            Quantity = i.Quantity,
                            TotalPrice = i.TotalPrice
                        };
                    })
                    .ToList()
            };
        }

        private async Task ExpirePendingBookingsAsync(CancellationToken ct)
        {
            var now = DateTime.UtcNow;
            var expiredBookings = await unitOfWork.Bookings.FindAsync(b =>
                b.Status == BookingStatus.Pending &&
                b.ExpiresAt.HasValue &&
                b.ExpiresAt <= now, ct);

            if (expiredBookings.Count == 0)
            {
                return;
            }

            foreach (var booking in expiredBookings)
            {
                booking.Status = BookingStatus.Expired;
                unitOfWork.Bookings.Update(booking);
            }

            await unitOfWork.TryCompeleteAsync(ct);
        }

        private async Task<Dictionary<int, int>> GetActiveBookedQuantitiesAsync(IEnumerable<int> ticketTypeIds, int eventId, CancellationToken ct)
        {
            var ticketTypeIdSet = ticketTypeIds.ToHashSet();
            if (ticketTypeIdSet.Count == 0)
            {
                return [];
            }

            var now = DateTime.UtcNow;
            var activeBookingIds = (await unitOfWork.Bookings.FindAsync(b =>
                    b.EventId == eventId &&
                    (b.Status == BookingStatus.Confirmed ||
                     (b.Status == BookingStatus.Pending && (!b.ExpiresAt.HasValue || b.ExpiresAt > now))), ct))
                .Select(b => b.Id)
                .ToHashSet();

            if (activeBookingIds.Count == 0)
            {
                return [];
            }

            return (await unitOfWork.BookingItems.FindAsync(i => ticketTypeIdSet.Contains(i.TicketTypeId), ct))
                .Where(i => activeBookingIds.Contains(i.BookingId))
                .GroupBy(i => i.TicketTypeId)
                .ToDictionary(g => g.Key, g => g.Sum(i => i.Quantity));
        }
    }
}
