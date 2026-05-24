using EventBookingSystem.DomainEvents.Events;
using EventBookingSystem.Hubs;
using EventBookingSystem.Services.Interfaces;
using EventBookingSystem.ViewModels;
using Microsoft.AspNetCore.SignalR;

namespace EventBookingSystem.DomainEvents.Handlers
{
    public class BookingCreatedNotificationHandler(
        INotificationService notificationService, 
        ILogger<BookingCreatedNotificationHandler> logger,
        IHubContext<NotificationHub, INotificationClient> hubContext
        ) : IEventHandler<BookingCreatedEvent>
    {
        public async Task HandleAsync(BookingCreatedEvent @event, CancellationToken ct = default)
        {
            var viewModel = new NotificationCreationViewModel();
            viewModel.Title = "Booking Created";
            viewModel.Message = $"A booking is created for event {@event.EventName}";

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
