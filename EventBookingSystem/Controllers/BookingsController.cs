using EventBookingSystem.Extensions;
using EventBookingSystem.Models;
using EventBookingSystem.Services.Interfaces;
using EventBookingSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventBookingSystem.Controllers
{
    public class BookingsController(IBookingService bookingService) : Controller
    {
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var bookings = await bookingService.GetUserBookingsAsync(User.GetCurrentUserId(), ct);
            return View(bookings);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Details(int id, CancellationToken ct)
        {
            var booking = await bookingService.GetUserBookingDetailsAsync(id, User.GetCurrentUserId(), ct);
            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Status(int id, CancellationToken ct)
        {
            var result = await bookingService.GetUserBookingStatusAsync(id, User.GetCurrentUserId(), ct);
            if (!result.Succeeded)
            {
                return NotFound();
            }

            return Json(new
            {
                status = result.Value.ToString(),
                isConfirmed = result.Value == BookingStatus.Confirmed
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateBookingViewModel model, CancellationToken ct)
        {
            if (model.EventId <= 0)
            {
                return BadRequest();
            }

            if (User.Identity?.IsAuthenticated != true)
            {
                var returnUrl = Url.Action("Details", "Events", new { id = model.EventId }) ?? "/";
                return RedirectToAction("Login", "Auth", new { returnUrl });
            }

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Check your ticket quantities and try again.";
                return RedirectToAction("Details", "Events", new { id = model.EventId });
            }

            var result = await bookingService.CreateBookingAsync(model, User.GetCurrentUserId(), ct);
            if (!result.Succeeded || result.Value == 0)
            {
                TempData["ErrorMessage"] = result.ErrorMessage ?? "Could not create booking.";
                return RedirectToAction("Details", "Events", new { id = model.EventId });
            }

            return RedirectToAction(nameof(Details), new { id = result.Value });
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id, CancellationToken ct)
        {
            var result = await bookingService.CancelUserBookingAsync(id, User.GetCurrentUserId(), ct);
            if (!result.Succeeded)
            {
                TempData["BookingError"] = result.ErrorMessage ?? "Could not cancel booking.";
                return RedirectToAction(nameof(Details), new { id });
            }

            TempData["BookingMessage"] = "Booking cancelled successfully.";
            return RedirectToAction(nameof(Details), new { id });
        }

    }
}
