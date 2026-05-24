using EventBookingSystem.DomainEvents.Events;
using EventBookingSystem.Hubs;
using EventBookingSystem.Services.Interfaces;
using EventBookingSystem.ViewModels;
using Microsoft.AspNetCore.SignalR;

namespace EventBookingSystem.DomainEvents.Handlers
{
    public class BookingConfirmationNotificationHandler(
        INotificationService notificationService,
        ILogger<BookingConfirmationNotificationHandler> logger,
        IHubContext<NotificationHub, INotificationClient> hubContext
        ) : IEventHandler<BookingConfirmedEvent>
    {
        public async Task HandleAsync(BookingConfirmedEvent @event, CancellationToken ct = default)
        {
            var viewModel = new NotificationCreationViewModel();
            viewModel.Title = "Booking Confirmed";
            viewModel.Message = 
                $"Payment for event '{@event.EventName}' was successful.";

            var result = await notificationService.CreateAsync(viewModel, @event.UserId, ct);

            if (!result.Succeeded || result.Value is null)
            {
                logger.LogError("Failed to create notification due to {CauseOfFailure} or value in result object is null.", result.ErrorMessage);
                return;
            }

            var clientViewModel = new NotificationViewModel();
            clientViewModel.Title = result.Value.Title;
            clientViewModel.Message = result.Value.Message;
            clientViewModel.IsRead = result.Value.IsRead;
            clientViewModel.CreatedDate = result.Value.CreatedDate;

            await hubContext.Clients.User(@event.UserId.ToString()).Notify(clientViewModel);
        }
    }
}
