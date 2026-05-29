using EventBookingSystem.Configuration;
using EventBookingSystem.DomainEvents.Events;
using EventBookingSystem.Services.Interfaces;
using Microsoft.AspNetCore.Builder.Extensions;
using Microsoft.Extensions.Options;

namespace EventBookingSystem.DomainEvents.Handlers
{
    public class BookingCreatedEmailHandler : IEventHandler<BookingCreatedEvent>
    {
        private readonly IEmailService _emailService;
        private readonly IOptions<ApplicationSettings> _appOptions;

        public BookingCreatedEmailHandler(
            IEmailService emailService,
            IOptions<ApplicationSettings> appOptions)
        {
            _emailService = emailService;
            _appOptions = appOptions;
        }
        public async Task HandleAsync(BookingCreatedEvent @event, CancellationToken ct = default)
        {
            var filepath = @"DomainEvents//Templates//BookingCreatedEmailTemplate.html";
            var str = new StreamReader(filepath);
            var mailText = await str.ReadToEndAsync();
            str.Close();

            mailText = mailText.Replace("[SiteUrl]", _appOptions.Value.BaseUrl)
                .Replace("[BookingId]", @event.BookingId.ToString());

            await _emailService.SendEmailAsync(
                @event.UserEmail,
                "Booking Creation",
                mailText,
                null,
                ct
                );
        }
    }
}
