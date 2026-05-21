using EventBookingSystem.DomainEvents.Events;
using EventBookingSystem.Services.Interfaces;

namespace EventBookingSystem.DomainEvents.Handlers
{
    public class BookingConfirmedEmailHandler : IEventHandler<BookingConfirmedEvent>
    {
        private readonly IEmailService _emailService;

        public BookingConfirmedEmailHandler(
            IEmailService emailService)
        {
            _emailService = emailService;
        }
        public async Task HandleAsync(BookingConfirmedEvent @event)
        {
            var filepath = $"{Directory.GetCurrentDirectory()}\\DomainEvents\\Templates\\BookingConfirmedEmailTemplate.html";
            var str = new StreamReader(filepath);
            var mailText = await str.ReadToEndAsync();
            str.Close();

            mailText = mailText.Replace("[SiteUrl]", "https://localhost:7235")
                .Replace("[BookingId]", @event.BookingId.ToString())
                .Replace("[EventName]", @event.EventName)
                .Replace("[EventDate]", @event.EventDate.ToString("f"))
                .Replace("[EventVenue]", @event.EventVenue)
                .Replace("[TicketCount]", @event.TicketCount.ToString())
                .Replace("[TotalAmount]", @event.TotalAmount.ToString("C"));

            await _emailService.SendEmailAsync(
                @event.UserEmail,
                "Booking Confirmation",
                mailText
                );
        }
    }
}
