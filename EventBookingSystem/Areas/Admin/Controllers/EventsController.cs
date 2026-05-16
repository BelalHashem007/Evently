using EventBookingSystem.Areas.Admin.ViewModels;
using EventBookingSystem.Common.Results;
using EventBookingSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventBookingSystem.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class EventsController(IEventService eventService) : Controller
    {
        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var events = await eventService.GetAdminEventListAsync(ct);
            return View(events);
        }

        [HttpGet]
        public async Task<IActionResult> Create(CancellationToken ct)
        {
            var model = await eventService.GetCreateFormAsync(ct);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EventFormViewModel model, CancellationToken ct)
        {
            RemoveValidationErrorsForRemovedTickets(model);
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await eventService.CreateEventAsync(model, ct);
            if (!result.Succeeded)
            {
                AddServiceError(result);
                return View(model);
            }

            return RedirectToAction("Index", "Home", new {area=""});
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id, CancellationToken ct)
        {
            var model = await eventService.GetEditFormAsync(id, ct);
            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EventFormViewModel model, CancellationToken ct)
        {
            RemoveValidationErrorsForRemovedTickets(model);
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await eventService.UpdateEventAsync(model, ct);
            if (!result.Succeeded)
            {
                AddServiceError(result);
                return View(model);
            }

            return RedirectToAction("Details", "Events" , new { id = model.Id, area="" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id, CancellationToken ct)
        {
            var result = await eventService.CancelEventAsync(id, ct);
            if (!result.Succeeded)
            {
                TempData["ErrorMessage"] = result.ErrorMessage;
            }

            return RedirectToAction("Details", new { id, area="" });
        }

        //helpers
        private void AddServiceError(Result result)
        {
            if (!string.IsNullOrWhiteSpace(result.ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage);
                return;
            }

            if (result.ValidationErrors == null)
            {
                return;
            }

            foreach (var error in result.ValidationErrors)
            {
                foreach (var message in error.Value)
                {
                    ModelState.AddModelError(error.Key, message);
                }
            }
        }

        private void RemoveValidationErrorsForRemovedTickets(EventFormViewModel model)
        {
            for (int i = 0; i < model.TicketTypes.Count; i++)
            {
                if (model.TicketTypes[i].Remove)
                {
                    ModelState.Remove($"TicketTypes[{i}].Name");
                    ModelState.Remove($"TicketTypes[{i}].Price");
                    ModelState.Remove($"TicketTypes[{i}].Quantity");
                }
            }
        }
    }
}
