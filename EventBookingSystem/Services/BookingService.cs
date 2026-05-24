using EventBookingSystem.Areas.Admin.ViewModels;
using EventBookingSystem.Common.Results;
using EventBookingSystem.DomainEvents.Dispatcher;
using EventBookingSystem.DomainEvents.Events;
using EventBookingSystem.Extensions;
using EventBookingSystem.Models;
using EventBookingSystem.Repositories.Interfaces;
using EventBookingSystem.Services.Interfaces;
using EventBookingSystem.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EventBookingSystem.Services
{
    public class BookingService(
        IUnitOfWork unitOfWork, 
        UserManager<ApplicationUser> userManager, 
        IEventDispatcher eventDispatcher,
        ILogger<BookingService> logger
        ) : IBookingService
    {
        private static readonly TimeSpan PendingBookingLifetime = TimeSpan.FromMinutes(15);

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

            var now = DateTime.UtcNow;
            return bookings
                .Select(b => new AdminBookingListItemViewModel
                {
                    Id = b.Id,
                    EventName = eventsById.TryGetValue(b.EventId, out var eventItem) ? eventItem.Name : "Unknown event",
                    UserName = usersById.TryGetValue(b.UserId, out var user) ? user.UserName ?? user.Email ?? "Unknown user" : "Unknown user",
                    TotalPrice = b.TotalPrice,
                    CreatedDate = b.CreatedDate,
                    Status = GetEffectiveStatus(b, now),
                    ExpiresAt = b.ExpiresAt
                })
                .ToList();
        }

        public async Task<Result<int>> CreateBookingAsync(CreateBookingViewModel model, ClaimsPrincipal user, CancellationToken ct = default)
        {
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

            var now = DateTime.UtcNow;
            var booking = new Booking
            {
                UserId = user.GetCurrentUserId(),
                EventId = model.EventId,
                CreatedDate = now,
                Status = BookingStatus.Pending,
                ExpiresAt = now.Add(PendingBookingLifetime)
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
            var userEmail = user.FindFirstValue(ClaimTypes.Email);

            if (dbResult.Succeeded)
            {
                if (userEmail is not null)
                    await eventDispatcher.PublishAsync(new BookingCreatedEvent(booking.Id, booking.UserId, userEmail, eventItem.Name));
                else logger.LogError("Failed to publish event 'BookingCreatedEvent' because userEmail is null");
            }

            return dbResult.Succeeded
                ? Result<int>.Success(booking.Id)
                : Result<int>.Failure(dbResult.ErrorMessage ?? "Could not create booking.");
        }

        public async Task<IReadOnlyList<BookingListItemViewModel>> GetUserBookingsAsync(int userId, CancellationToken ct = default)
        {
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

            var now = DateTime.UtcNow;
            return bookings
                .Select(b => new BookingListItemViewModel
                {
                    Id = b.Id,
                    EventName = eventsById.TryGetValue(b.EventId, out var eventItem) ? eventItem.Name : "Unknown event",
                    EventDate = eventsById.TryGetValue(b.EventId, out eventItem) ? eventItem.Date : default,
                    Status = GetEffectiveStatus(b, now),
                    CreatedDate = b.CreatedDate,
                    ExpiresAt = b.ExpiresAt,
                    TotalPrice = b.TotalPrice
                })
                .ToList();
        }

        public async Task<BookingDetailsViewModel?> GetUserBookingDetailsAsync(int bookingId, int userId, CancellationToken ct = default)
        {
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

            var now = DateTime.UtcNow;
            return new BookingDetailsViewModel
            {
                Id = booking.Id,
                EventName = eventItem?.Name ?? "Unknown event",
                EventDate = eventItem?.Date ?? default,
                Venue = eventItem?.Venue ?? "Unknown venue",
                Status = GetEffectiveStatus(booking, now),
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

        public async Task<Result<BookingStatus>> GetUserBookingStatusAsync(int bookingId, int userId, CancellationToken ct = default)
        {
            var booking = await unitOfWork.Bookings.GetByIdAsync(bookingId, ct);
            if (booking == null || booking.UserId != userId)
            {
                return Result<BookingStatus>.Failure("Booking was not found.");
            }

            return Result<BookingStatus>.Success(GetEffectiveStatus(booking, DateTime.UtcNow));
        }

        public async Task<Result> CancelUserBookingAsync(int bookingId, int userId, CancellationToken ct = default)
        {
            var booking = await unitOfWork.Bookings.GetByIdAsync(bookingId, ct);
            if (booking == null || booking.UserId != userId)
            {
                return Result.Failure("Booking was not found.");
            }

            if (GetEffectiveStatus(booking, DateTime.UtcNow) != BookingStatus.Pending)
            {
                return Result.Failure("Only pending bookings can be cancelled.");
            }

            booking.Status = BookingStatus.Cancelled;
            unitOfWork.Bookings.Update(booking);

            var dbResult = await unitOfWork.TryCompeleteAsync(ct);
            return dbResult.Succeeded
                ? Result.Success()
                : Result.Failure(dbResult.ErrorMessage ?? "Could not cancel booking.");
        }

        public async Task ExpirePendingBookingsAsync(CancellationToken ct = default)
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

        private static BookingStatus GetEffectiveStatus(Booking booking, DateTime now)
        {
            return booking.Status == BookingStatus.Pending &&
                   booking.ExpiresAt.HasValue &&
                   booking.ExpiresAt <= now
                ? BookingStatus.Expired
                : booking.Status;
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
