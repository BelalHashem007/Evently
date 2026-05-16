using EventBookingSystem.Areas.Admin.ViewModels;

namespace EventBookingSystem.Services.Interfaces
{
    public interface IUserService
    {
        Task<IReadOnlyList<AdminUserListItemViewModel>> GetAdminUserListAsync(CancellationToken ct = default);
    }
}
