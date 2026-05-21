using EventBookingSystem.Services.Interfaces;
using EventBookingSystem.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace EventBookingSystem.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmailController(IEmailService emailService) : ControllerBase
    {
        [HttpPost]
        [Route("send")]
        public async Task<IActionResult> SendEmail([FromForm] EmailRequestDto dto)
        {
            await emailService.SendEmailAsync(dto.ToEmail, dto.Subject, dto.Body, dto.Attachments);
            return Ok();
        }
    }
}
