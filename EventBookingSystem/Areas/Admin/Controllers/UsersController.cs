using EventBookingSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventBookingSystem.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UsersController(IUserService userService) : Controller
    {
        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var users = await userService.GetAdminUserListAsync(ct);
            return View(users);
        }
    }
}
