namespace EventBookingSystem.DomainEvents.Events
{
    public record BookingCreatedEvent(int BookingId, string UserEmail, string EventName)
    {
    }
}
