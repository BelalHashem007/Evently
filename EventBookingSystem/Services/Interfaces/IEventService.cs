using EventBookingSystem.ViewModels;

namespace EventBookingSystem.Services.Interfaces
{
    public interface IEventService
    {
        Task<IReadOnlyList<EventCardViewModel>> GetEventCardsAsync(CancellationToken ct = default);
        Task<EventDetailsViewModel?> GetEventDetailsAsync(int id, CancellationToken ct = default);
    }
}
