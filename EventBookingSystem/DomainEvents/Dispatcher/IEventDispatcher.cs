namespace EventBookingSystem.DomainEvents.Dispatcher
{
    public interface IEventDispatcher
    {
        ValueTask PublishAsync<TEvent>(TEvent @event, CancellationToken ct = default);
    }
}
