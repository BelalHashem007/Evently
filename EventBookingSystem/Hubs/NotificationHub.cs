using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace EventBookingSystem.Hubs
{
    [Authorize]
    public class NotificationHub() : Hub<INotificationClient>
    {
    }
}
