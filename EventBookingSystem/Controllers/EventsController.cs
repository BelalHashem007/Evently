using EventBookingSystem.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EventBookingSystem.Controllers
{
    public class EventsController(IEventService eventService) : Controller
    {
        [HttpGet]
        public async Task<IActionResult> Details(int id, CancellationToken ct)
        {
            var eventDetails = await eventService.GetEventDetailsAsync(id, ct);
            if (eventDetails == null)
            {
                return NotFound();
            }

            return View(eventDetails);
        }
    }
}
