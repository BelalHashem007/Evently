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
        public async Task<IActionResult> CheckOut(int bookingId)
        {
            var successUrl = linkGenerator.GetUriByAction(nameof(Success), "Payments", null, HttpContext.Request.Scheme, HttpContext.Request.Host);
            var cancelUrl = linkGenerator.GetUriByAction(nameof(Cancel), "Payments", null, HttpContext.Request.Scheme, HttpContext.Request.Host);

            var result = await paymentService.GetStripeSessionAsync(bookingId, successUrl, cancelUrl);

            if (!result.Succeeded || result.Value == null)
            {
                TempData["StripeError"] = result.ErrorMessage ?? "Unknown Error";
                return RedirectToAction("Details", "Bookings", new { Id = bookingId});
            }

            return Redirect(result.Value.Url);
        }

        [HttpGet]
        public IActionResult Success()
        {
            return Ok("Payment Success");
        }

        [HttpGet]
        public IActionResult Cancel()
        {
            return Ok("Payment Cancelled");
        }
    }
}
