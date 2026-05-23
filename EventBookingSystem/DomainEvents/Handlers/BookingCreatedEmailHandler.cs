using EventBookingSystem.DomainEvents.Events;
using EventBookingSystem.Services.Interfaces;

namespace EventBookingSystem.DomainEvents.Handlers
{
    public class BookingCreatedEmailHandler : IEventHandler<BookingCreatedEvent>
    {
        private readonly IEmailService _emailService;

        public BookingCreatedEmailHandler(
            IEmailService emailService)
        {
            _emailService = emailService;
        }
        public async Task HandleAsync(BookingCreatedEvent @event, CancellationToken ct = default)
        {
            var filepath = $"{Directory.GetCurrentDirectory()}\\DomainEvents\\Templates\\BookingCreatedEmailTemplate.html";
            var str = new StreamReader(filepath);
            var mailText = await str.ReadToEndAsync();
            str.Close();

            mailText = mailText.Replace("[SiteUrl]", "https://localhost:7235")
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
