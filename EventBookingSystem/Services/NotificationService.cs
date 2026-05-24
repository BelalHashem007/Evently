using EventBookingSystem.Common.Results;
using EventBookingSystem.Models;
using EventBookingSystem.Repositories.Interfaces;
using EventBookingSystem.Services.Interfaces;
using EventBookingSystem.ViewModels;

namespace EventBookingSystem.Services
{
    public class NotificationService(IUnitOfWork unitOfWork) : INotificationService
    {
        private const int DefaultNotificationPageSize = 10;
        private const int MaxNotificationPageSize = 50;

        public async Task<Result<Notification>> CreateAsync(NotificationCreationViewModel viewModel, int userId, CancellationToken ct = default)
        {
            var validator = new NotificationCreationViewModelValidator();
            if (!validator.Validate(viewModel))
                return Result<Notification>.Failure("Invalid input");

            var newNotification = new Notification
            {
                Title = viewModel.Title,
                Message = viewModel.Message,
                UserId = userId,
                CreatedDate = DateTime.UtcNow
            };

            await unitOfWork.Notifications.AddAsync(newNotification, ct);
            var dbResult = await unitOfWork.TryCompeleteAsync(ct);

            return dbResult.Succeeded ?
                Result<Notification>.Success(newNotification)
                : Result<Notification>.Failure("Failed to save notification to DB");
        }

        public async Task<NotificationPageViewModel> GetNotifications(int userId, int skip, int take, CancellationToken ct = default)
        {
            var normalizedSkip = Math.Max(0, skip);
            var normalizedTake = take <= 0 ? DefaultNotificationPageSize : Math.Min(take, MaxNotificationPageSize);
            var notifications = await unitOfWork.Notifications.GetLatestForUserAsync(userId, normalizedSkip, normalizedTake, ct);

            return new NotificationPageViewModel
            {
                Notifications = notifications
                    .Take(normalizedTake)
                    .Select(n => new NotificationViewModel
                {
                    Title = n.Title,
                    Message = n.Message,
                    IsRead = n.IsRead,
                    CreatedDate = DateTime.SpecifyKind(n.CreatedDate, DateTimeKind.Utc)
                }),
                IsEnd = notifications.Count <= normalizedTake
            };
        }

        public async Task<Result> UpdateReadStatus(int userId, CancellationToken ct = default)
        {
            try
            {
                await unitOfWork.Notifications.UpdateReadStatus(userId, ct);
                return Result.Success();
            }
            catch(Exception)
            {
                return Result.Failure("Failed to update DB");
            }
        }
    }
}
