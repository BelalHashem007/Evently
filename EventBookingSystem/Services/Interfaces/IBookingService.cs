using EventBookingSystem.Areas.Admin.ViewModels;

namespace EventBookingSystem.Services.Interfaces
{
    public interface IBookingService
    {
        Task<IReadOnlyList<AdminBookingListItemViewModel>> GetAdminBookingListAsync(CancellationToken ct = default);
    }
}
