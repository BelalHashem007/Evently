using EventBookingSystem.Extensions;
using EventBookingSystem.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EventBookingSystem.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WebhookController(IWebhookService webhookService) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var body = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            if (!Request.Headers.TryGetValue("Stripe-Signature", out var signatureHeader))
                return BadRequest();

            var result = await webhookService.UpdateBooking(body, signatureHeader.ToString(), ct);

            if (result.ErrorMessage == "Stripe Error")
                return BadRequest();
            else if (result.Succeeded)
                return Ok();
            else return StatusCode(500);
        }
    }
}
