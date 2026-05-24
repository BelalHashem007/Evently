using EventBookingSystem.Common.Results;
using EventBookingSystem.Models;
using EventBookingSystem.ViewModels;

namespace EventBookingSystem.Services.Interfaces
{
    public interface INotificationService
    {
        public Task<Result<Notification>> CreateAsync(NotificationCreationViewModel viewModel, int userId, CancellationToken ct = default);
        public Task<NotificationPageViewModel> GetNotifications(int userId, int skip, int take, CancellationToken ct = default);
        public Task<Result> UpdateReadStatus(int userId, CancellationToken ct = default);
    }
}
