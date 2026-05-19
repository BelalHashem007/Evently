using EventBookingSystem.Extensions;
using EventBookingSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventBookingSystem.Controllers
{
    [Authorize(Roles = "User")]
    public class PaymentsController(IPaymentService paymentService, LinkGenerator linkGenerator) : Controller
    {

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckOut(int bookingId, CancellationToken ct)
        {
            var routeValues = new { bookingId };
            var successUrl = linkGenerator.GetUriByAction(nameof(Success), "Payments", routeValues, HttpContext.Request.Scheme, HttpContext.Request.Host);
            var cancelUrl = linkGenerator.GetUriByAction(nameof(Cancel), "Payments", routeValues, HttpContext.Request.Scheme, HttpContext.Request.Host);

            var result = await paymentService.GetStripeSessionAsync(bookingId, User.GetCurrentUserId(), successUrl!, cancelUrl!, ct);

            if (!result.Succeeded || result.Value == null)
            {
                TempData["StripeError"] = result.ErrorMessage ?? "Unknown Error";
                return RedirectToAction("Details", "Bookings", new { id = bookingId});
            }

            return Redirect(result.Value.Url);
        }

        [HttpGet]
        public IActionResult Success(int bookingId)
        {
            TempData["StripeMessage"] = "Payment received. Your booking will update once Stripe confirms it.";
            TempData["PollPaymentStatus"] = "true";
            return RedirectToAction("Details", "Bookings", new { id = bookingId });
        }

        [HttpGet]
        public IActionResult Cancel(int bookingId)
        {
            TempData["StripeError"] = "Payment was cancelled. You can try again before the reservation expires.";
            return RedirectToAction("Details", "Bookings", new { id = bookingId });
        }
    }
}
