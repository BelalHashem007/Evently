namespace EventBookingSystem.DomainEvents.Events
{
    public record BookingCreatedEvent(int BookingId, int UserId, string UserEmail, string EventName)
    {
    }
}
