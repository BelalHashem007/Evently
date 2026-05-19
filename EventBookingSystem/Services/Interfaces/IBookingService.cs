using EventBookingSystem.Areas.Admin.ViewModels;
using EventBookingSystem.Common.Results;
using EventBookingSystem.ViewModels;

namespace EventBookingSystem.Services.Interfaces
{
    public interface IBookingService
    {
        Task<IReadOnlyList<AdminBookingListItemViewModel>> GetAdminBookingListAsync(CancellationToken ct = default);
        Task<Result<int>> CreateBookingAsync(CreateBookingViewModel model, int userId, CancellationToken ct = default);
        Task<IReadOnlyList<BookingListItemViewModel>> GetUserBookingsAsync(int userId, CancellationToken ct = default);
        Task<BookingDetailsViewModel?> GetUserBookingDetailsAsync(int bookingId, int userId, CancellationToken ct = default);
        Task ExpirePendingBookingsAsync(CancellationToken ct = default);
    }
}
