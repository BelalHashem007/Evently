using EventBookingSystem.Configuration;
using EventBookingSystem.DomainEvents.Events;
using EventBookingSystem.Services.Interfaces;
using Microsoft.AspNetCore.Builder.Extensions;
using Microsoft.Extensions.Options;

namespace EventBookingSystem.DomainEvents.Handlers
{
    public class BookingConfirmedEmailHandler : IEventHandler<BookingConfirmedEvent>
    {
        private readonly IEmailService _emailService;
        private readonly IOptions<ApplicationSettings> _appOptions;

        public BookingConfirmedEmailHandler(
            IEmailService emailService,
            IOptions<ApplicationSettings> appOptions)
        {
            _emailService = emailService;
            _appOptions = appOptions;
        }
        public async Task HandleAsync(BookingConfirmedEvent @event, CancellationToken ct = default)
        {
            var filepath = @"DomainEvents//Templates//BookingConfirmedEmailTemplate.html";
            var str = new StreamReader(filepath);
            var mailText = await str.ReadToEndAsync();
            str.Close();

            mailText = mailText.Replace("[SiteUrl]", _appOptions.Value.BaseUrl)
                .Replace("[BookingId]", @event.BookingId.ToString())
                .Replace("[EventName]", @event.EventName)
                .Replace("[EventDate]", @event.EventDate.ToString("f"))
                .Replace("[EventVenue]", @event.EventVenue)
                .Replace("[TicketCount]", @event.TicketCount.ToString())
                .Replace("[TotalAmount]", @event.TotalAmount.ToString("C"));

            await _emailService.SendEmailAsync(
                @event.UserEmail,
                "Booking Confirmation",
                mailText,
                null,
                ct
                );
        }
    }
}
