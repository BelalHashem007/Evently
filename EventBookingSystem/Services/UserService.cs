using EventBookingSystem.Areas.Admin.ViewModels;
using EventBookingSystem.Models;
using EventBookingSystem.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EventBookingSystem.Services
{
    public class UserService(UserManager<ApplicationUser> userManager) : IUserService
    {
        public async Task<IReadOnlyList<AdminUserListItemViewModel>> GetAdminUserListAsync(CancellationToken ct = default)
        {
            return await userManager.Users
                .OrderBy(u => u.Id)
                .Select(u => new AdminUserListItemViewModel
                {
                    Id = u.Id,
                    UserName = u.UserName ?? string.Empty,
                    Email = u.Email ?? string.Empty,
                    PhoneNumber = u.PhoneNumber ?? string.Empty
                })
                .ToListAsync(ct);
        }
    }
}
