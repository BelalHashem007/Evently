using EventBookingSystem.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EventBookingSystem.Controllers
{
    public class AuthController(IAuthService authService) : Controller
    {
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await authService.LogoutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}
