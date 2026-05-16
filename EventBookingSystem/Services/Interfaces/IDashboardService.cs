using EventBookingSystem.Areas.Admin.ViewModels;

namespace EventBookingSystem.Services.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardViewModel> GetDashboardAsync(CancellationToken ct = default);
    }
}
