using EventBookingSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventBookingSystem.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class HomeController(IDashboardService dashboardService) : Controller
    {
        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var model = await dashboardService.GetDashboardAsync(ct);
            return View(model);
        }
    }
}
