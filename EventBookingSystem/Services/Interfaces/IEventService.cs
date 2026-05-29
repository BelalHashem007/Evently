using EventBookingSystem.Areas.Admin.ViewModels;
using EventBookingSystem.Common.Results;
using EventBookingSystem.ViewModels;

namespace EventBookingSystem.Services.Interfaces
{
    public interface IEventService
    {
        Task<PaginatedViewModel<EventCardViewModel>> GetEventCardsAsync(int page, CancellationToken ct = default);
        Task<IReadOnlyList<AdminEventListItemViewModel>> GetAdminEventListAsync(CancellationToken ct = default);
        Task<EventDetailsViewModel?> GetEventDetailsAsync(int id, CancellationToken ct = default);
        Task<EventFormViewModel> GetCreateFormAsync(CancellationToken ct = default);
        Task<EventFormViewModel?> GetEditFormAsync(int id, CancellationToken ct = default);
        Task<Result> CreateEventAsync(EventFormViewModel model, CancellationToken ct = default);
        Task<Result> UpdateEventAsync(EventFormViewModel model, CancellationToken ct = default);
        Task<Result> CancelEventAsync(int id, CancellationToken ct = default);
    }
}
