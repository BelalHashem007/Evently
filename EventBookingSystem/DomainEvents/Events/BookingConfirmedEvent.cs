namespace EventBookingSystem.DomainEvents.Events
{
    public record BookingConfirmedEvent (
        int BookingId,
        string UserEmail,
        string EventName,
        DateTime EventDate,
        string EventVenue,
        int TicketCount,
        decimal TotalAmount
        )
    {
    }
}
