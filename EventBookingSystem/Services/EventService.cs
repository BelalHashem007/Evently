using EventBookingSystem.Areas.Admin.ViewModels;
using EventBookingSystem.Common.Results;
using EventBookingSystem.Models;
using EventBookingSystem.Repositories.Interfaces;
using EventBookingSystem.Services.Interfaces;
using EventBookingSystem.ViewModels;

namespace EventBookingSystem.Services
{
    public class EventService(IUnitOfWork unitOfWork) : IEventService
    {
        public async Task<PaginatedViewModel<EventCardViewModel>> GetEventCardsAsync(int page, CancellationToken ct = default)
        {
            var take = 15;
            var skip = (page - 1) * take;
            var events = await unitOfWork.Events.FindWithPaginationAsync(e => !e.IsCancelled, e => e.Date, skip, take, false, ct);

            if (events.Count == 0)
            {
                return new PaginatedViewModel<EventCardViewModel>
                {
                    Items = [],
                    Page = page,
                    TotalItems = 0,
                    MaxItemsPerPage = take,
                };
            }

            var eventsTotalCount = await unitOfWork.Events.CountAsync(e => !e.IsCancelled, ct);
            var eventIds = events.Select(e => e.Id).ToList();
            var ticketTypesByEventId = (await unitOfWork.TicketTypes.FindAsync(t => eventIds.Contains(t.EventId), ct))
                .GroupBy(t => t.EventId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var eventsList = events
                .Select(e => new EventCardViewModel
                {
                    Id = e.Id,
                    Name = e.Name,
                    Date = e.Date,
                    Venue = e.Venue,
                    ImageUrl = e.ImageUrl,
                    PriceRange = GetPriceRange(ticketTypesByEventId.GetValueOrDefault(e.Id) ?? [])
                })
                .ToList();

            return new PaginatedViewModel<EventCardViewModel>
            {
                Items = eventsList,
                Page = page,
                TotalItems = eventsTotalCount,
                MaxItemsPerPage = take
            };
        }

        public async Task<IReadOnlyList<AdminEventListItemViewModel>> GetAdminEventListAsync(CancellationToken ct = default)
        {
            var events = (await unitOfWork.Events.GetAllAsync(ct))
                .OrderByDescending(e => e.Date)
                .ToList();

            if (events.Count == 0)
            {
                return [];
            }

            var eventIds = events.Select(e => e.Id).ToList();
            var ticketTypeCounts = (await unitOfWork.TicketTypes.FindAsync(t => eventIds.Contains(t.EventId), ct))
                .GroupBy(t => t.EventId)
                .ToDictionary(g => g.Key, g => g.Count());

            return events
                .Select(e => new AdminEventListItemViewModel
                {
                    Id = e.Id,
                    Name = e.Name,
                    Date = e.Date,
                    Venue = e.Venue,
                    IsCancelled = e.IsCancelled,
                    TicketTypeCount = ticketTypeCounts.GetValueOrDefault(e.Id)
                })
                .ToList();
        }

        public async Task<EventDetailsViewModel?> GetEventDetailsAsync(int id, CancellationToken ct = default)
        {
            var eventItem = await unitOfWork.Events.GetByIdAsync(id, ct);
            if (eventItem == null)
            {
                return null;
            }

            var ticketTypes = (await unitOfWork.TicketTypes.FindAsync(t => t.EventId == id, ct))
                .OrderByDescending(t => t.Price)
                .ToList();
            var ticketTypeIds = ticketTypes.Select(t => t.Id).ToList();
            var now = DateTime.UtcNow;
            var activeBookings = (await unitOfWork.Bookings.FindAsync(b =>
                    b.EventId == id &&
                    (b.Status == BookingStatus.Confirmed ||
                     (b.Status == BookingStatus.Pending && (b.ExpiresAt > now))), ct))
                .Select(b => b.Id)
                .ToHashSet();
            var bookedQuantities = ticketTypeIds.Count == 0
                ? new Dictionary<int, int>()
                : (await unitOfWork.BookingItems.FindAsync(i => ticketTypeIds.Contains(i.TicketTypeId), ct))
                    .Where(i => activeBookings.Contains(i.BookingId))
                    .GroupBy(i => i.TicketTypeId)
                    .ToDictionary(g => g.Key, g => g.Sum(i => i.Quantity));

            var ticketCards = ticketTypes
                .Select(t => new TicketTypeCardViewModel
                {
                    Id = t.Id,
                    Name = t.Name,
                    Price = t.Price,
                    Quantity = t.Quantity,
                    AvailableQuantity = Math.Max(0, t.Quantity - bookedQuantities.GetValueOrDefault(t.Id))
                })
                .ToList();

            return new EventDetailsViewModel
            {
                Id = eventItem.Id,
                Name = eventItem.Name,
                Date = eventItem.Date,
                Venue = eventItem.Venue,
                Description = eventItem.Description,
                ImageUrl = eventItem.ImageUrl,
                IsCancelled = eventItem.IsCancelled,
                TicketTypes = ticketCards
            };
        }

        public Task<EventFormViewModel> GetCreateFormAsync(CancellationToken ct = default)
        {
            return Task.FromResult(new EventFormViewModel
            {
                TicketTypes = [new TicketTypeFormViewModel()]
            });
        }

        public async Task<EventFormViewModel?> GetEditFormAsync(int id, CancellationToken ct = default)
        {
            var eventItem = await unitOfWork.Events.GetByIdAsync(id, ct);
            if (eventItem == null || eventItem.IsCancelled)
            {
                return null;
            }

            var ticketTypes = await unitOfWork.TicketTypes.FindAsync(t => t.EventId == id, ct);
            var ticketTypeIds = ticketTypes.Select(t => t.Id).ToList();
            var bookingItems = ticketTypeIds.Count == 0
                ? []
                : await unitOfWork.BookingItems.FindAsync(b => ticketTypeIds.Contains(b.TicketTypeId), ct);
            var ticketTypeIdsWithBookings = bookingItems
                .Select(b => b.TicketTypeId)
                .ToHashSet();

            return new EventFormViewModel
            {
                Id = eventItem.Id,
                Name = eventItem.Name,
                Description = eventItem.Description,
                Date = eventItem.Date,
                Venue = eventItem.Venue,
                ImageUrl = eventItem.ImageUrl,
                TicketTypes = ticketTypes
                    .OrderBy(t => t.Name)
                    .Select(t => new TicketTypeFormViewModel
                    {
                        Id = t.Id,
                        Name = t.Name,
                        Price = t.Price,
                        Quantity = t.Quantity,
                        HasBookingItems = ticketTypeIdsWithBookings.Contains(t.Id)
                    })
                    .ToList()
            };
        }

        public async Task<Result> CreateEventAsync(EventFormViewModel model, CancellationToken ct = default)
        {
            var activeTicketTypes = GetActiveTicketTypes(model.TicketTypes);
            if (activeTicketTypes.Count == 0)
            {
                return Result.Failure("At least one ticket type is required.");
            }

            var eventItem = new Event
            {
                Name = model.Name,
                Description = model.Description,
                Date = model.Date,
                Venue = model.Venue,
                ImageUrl = model.ImageUrl,
                IsCancelled = false
            };

            foreach (var ticketType in activeTicketTypes)
            {
                eventItem.TicketTypes.Add(new TicketType
                {
                    Name = ticketType.Name,
                    Price = ticketType.Price,
                    Quantity = ticketType.Quantity
                });
            }

            await unitOfWork.Events.AddAsync(eventItem, ct);
            Result dbResult = await unitOfWork.TryCompeleteAsync(ct);
            return dbResult;
        }

        public async Task<Result> UpdateEventAsync(EventFormViewModel model, CancellationToken ct = default)
        {
            var eventItem = await unitOfWork.Events.GetByIdAsync(model.Id, ct);
            if (eventItem == null)
            {
                return Result.Failure("Event was not found.");
            }

            if (eventItem.IsCancelled)
            {
                return Result.Failure("Cancelled events cannot be edited.");
            }

            var existingTicketTypes = (await unitOfWork.TicketTypes.FindAsync(t => t.EventId == model.Id, ct))
                .ToDictionary(t => t.Id);
            var ticketTypeIds = existingTicketTypes.Keys.ToList();
            var bookingItems = ticketTypeIds.Count == 0
                ? []
                : await unitOfWork.BookingItems.FindAsync(b => ticketTypeIds.Contains(b.TicketTypeId), ct);
            var ticketTypeIdsWithBookings = bookingItems
                .Select(b => b.TicketTypeId)
                .ToHashSet();

            var activeTicketTypes = GetActiveTicketTypes(model.TicketTypes);
            if (activeTicketTypes.Count == 0)
            {
                return Result.Failure("At least one ticket type is required.");
            }

            eventItem.Name = model.Name;
            eventItem.Description = model.Description;
            eventItem.Date = model.Date;
            eventItem.Venue = model.Venue;
            eventItem.ImageUrl = model.ImageUrl;
            unitOfWork.Events.Update(eventItem);

            foreach (var ticketTypeModel in model.TicketTypes)
            {
                if (ticketTypeModel.Id == 0)
                {
                    if (ticketTypeModel.Remove)
                    {
                        continue;
                    }

                    eventItem.TicketTypes.Add(new TicketType
                    {
                        Name = ticketTypeModel.Name,
                        Price = ticketTypeModel.Price,
                        Quantity = ticketTypeModel.Quantity
                    });
                    continue;
                }

                if (!existingTicketTypes.TryGetValue(ticketTypeModel.Id, out var existingTicketType))
                {
                    return Result.Failure("One or more ticket types could not be found for this event.");
                }

                if (ticketTypeModel.Remove)
                {
                    if (ticketTypeIdsWithBookings.Contains(ticketTypeModel.Id))
                    {
                        return Result.Failure($"Ticket type '{existingTicketType.Name}' cannot be removed because it has bookings.");
                    }

                    await unitOfWork.TicketTypes.DeleteAsync(ticketTypeModel.Id, ct);
                    continue;
                }

                if (ticketTypeIdsWithBookings.Contains(ticketTypeModel.Id) && existingTicketType.Price != ticketTypeModel.Price)
                {
                    return Result.Failure($"Ticket type '{existingTicketType.Name}' price cannot be changed after bookings exist.");
                }

                existingTicketType.Name = ticketTypeModel.Name;
                existingTicketType.Price = ticketTypeModel.Price;
                existingTicketType.Quantity = ticketTypeModel.Quantity;
                unitOfWork.TicketTypes.Update(existingTicketType);
            }

            Result dbResult = await unitOfWork.TryCompeleteAsync(ct);
            return dbResult;
        }

        public async Task<Result> CancelEventAsync(int id, CancellationToken ct = default)
        {
            var eventItem = await unitOfWork.Events.GetByIdAsync(id, ct);
            if (eventItem == null)
            {
                return Result.Failure("Event was not found.");
            }

            if (eventItem.IsCancelled)
            {
                return Result.Failure("Event is already cancelled.");
            }

            eventItem.IsCancelled = true;
            unitOfWork.Events.Update(eventItem);
            Result dbResult = await unitOfWork.TryCompeleteAsync(ct);
            return dbResult;
        }

        //helpers

        private static List<TicketTypeFormViewModel> GetActiveTicketTypes(IEnumerable<TicketTypeFormViewModel> ticketTypes)
        {
            return ticketTypes
                .Where(t => !t.Remove)
                .ToList();
        }

        private static string GetPriceRange(IReadOnlyCollection<TicketType> ticketTypes)
        {
            if (ticketTypes.Count == 0)
            {
                return "No tickets available";
            }

            var minPrice = ticketTypes.Min(t => t.Price);
            var maxPrice = ticketTypes.Max(t => t.Price);

            return minPrice == maxPrice
                ? $"EGP {minPrice}"
                : $"EGP {minPrice +" - "+ maxPrice}";
        }
    }
}
