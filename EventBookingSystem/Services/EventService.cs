using EventBookingSystem.Models;
using EventBookingSystem.Repositories.Interfaces;
using EventBookingSystem.Services.Interfaces;
using EventBookingSystem.ViewModels;

namespace EventBookingSystem.Services
{
    public class EventService(IUnitOfWork unitOfWork) : IEventService
    {
        public async Task<IReadOnlyList<EventCardViewModel>> GetEventCardsAsync(CancellationToken ct = default)
        {
            var events = (await unitOfWork.Events.GetAllAsync(ct))
                .OrderBy(e => e.Date)
                .ToList();

            if (events.Count == 0)
            {
                return [];
            }

            var eventIds = events.Select(e => e.Id).ToList();
            var ticketTypesByEventId = (await unitOfWork.TicketTypes.FindAsync(t => eventIds.Contains(t.EventId), ct))
                .GroupBy(t => t.EventId)
                .ToDictionary(g => g.Key, g => g.ToList());

            return events
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
                .Select(t => new TicketTypeCardViewModel
                {
                    Name = t.Name,
                    Price = t.Price,
                    Quantity = t.Quantity
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
                TicketTypes = ticketTypes
            };
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
