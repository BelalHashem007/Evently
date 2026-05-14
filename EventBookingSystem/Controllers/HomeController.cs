using EventBookingSystem.Models;
using EventBookingSystem.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace EventBookingSystem.Controllers
{
    public class HomeController(IEventService eventService) : Controller
    {
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var events = await eventService.GetEventCardsAsync(ct);
            return View(events);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
