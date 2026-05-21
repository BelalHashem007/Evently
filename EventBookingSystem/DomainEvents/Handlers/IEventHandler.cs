namespace EventBookingSystem.DomainEvents.Handlers
{
    public interface IEventHandler<TEvent>
    {
        Task HandleAsync(TEvent @event);
    }
}
