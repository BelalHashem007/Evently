using EventBookingSystem.Common.Results;
using EventBookingSystem.Services.Interfaces;
using EventBookingSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
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

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Create(CancellationToken ct)
        {
            var model = await eventService.GetCreateFormAsync(ct);
            return View(model);
        }

        [Authorize(Roles = "Admin")]
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

            return RedirectToAction("Index", "Home");
        }

        [Authorize(Roles = "Admin")]
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

        [Authorize(Roles = "Admin")]
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

            return RedirectToAction(nameof(Details), new { id = model.Id });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id, CancellationToken ct)
        {
            var result = await eventService.CancelEventAsync(id, ct);
            if (!result.Succeeded)
            {
                TempData["ErrorMessage"] = result.ErrorMessage;
            }

            return RedirectToAction(nameof(Details), new { id });
        }

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
            for (int i=0; i< model.TicketTypes.Count; i++)
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
