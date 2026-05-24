using EventBookingSystem.Extensions;
using EventBookingSystem.Hubs;
using EventBookingSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace EventBookingSystem.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("/api/[controller]")]
    public class NotificationController(
        INotificationService notificationService,
        IHubContext<NotificationHub, INotificationClient> hubContext,
        ILogger<NotificationController> logger
        ) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] int skip = 0, [FromQuery] int take = 10, CancellationToken ct = default)
        {
            var list = await notificationService.GetNotifications(User.GetCurrentUserId(), skip, take, ct);
            return Ok(list);
        }

        [HttpPatch]
        [Route("readAll")]
        public async Task<IActionResult> ReadAll(CancellationToken ct)
        {
            var userId = User.GetCurrentUserId();
            var result = await notificationService.UpdateReadStatus(userId, ct);
            if (!result.Succeeded)
                return StatusCode(500);

            try
            {
                await hubContext.Clients.User(userId.ToString()).UpdateReadStatus();
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to broadcast notification read status.");
            }

            return NoContent();
        }
    }
}
