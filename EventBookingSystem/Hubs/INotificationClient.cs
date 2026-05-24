using EventBookingSystem.ViewModels;

namespace EventBookingSystem.Hubs
{
    public interface INotificationClient
    {
        Task Notify(NotificationViewModel model);
        Task UpdateReadStatus();
    }
}
