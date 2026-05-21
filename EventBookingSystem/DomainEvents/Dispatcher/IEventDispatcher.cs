namespace EventBookingSystem.DomainEvents.Dispatcher
{
    public interface IEventDispatcher
    {
        Task PublishAsync<TEvent>(TEvent @event);
    }
}
