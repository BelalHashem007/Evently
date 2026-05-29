using EventBookingSystem.Areas.Admin.ViewModels;
using EventBookingSystem.Common.Results;
using EventBookingSystem.Models;
using EventBookingSystem.ViewModels;
using System.Security.Claims;

namespace EventBookingSystem.Services.Interfaces
{
    public interface IBookingService
    {
        Task<IReadOnlyList<AdminBookingListItemViewModel>> GetAdminBookingListAsync(CancellationToken ct = default);
        Task<Result<int>> CreateBookingAsync(CreateBookingViewModel model, ClaimsPrincipal user, CancellationToken ct = default);
        Task<PaginatedViewModel<BookingListItemViewModel>> GetUserBookingsAsync(int page, int userId, CancellationToken ct = default);
        Task<BookingDetailsViewModel?> GetUserBookingDetailsAsync(int bookingId, int userId, CancellationToken ct = default);
        Task<Result<BookingStatus>> GetUserBookingStatusAsync(int bookingId, int userId, CancellationToken ct = default);
        Task<Result> CancelUserBookingAsync(int bookingId, int userId, CancellationToken ct = default);
        Task ExpirePendingBookingsAsync(CancellationToken ct = default);
    }
}
