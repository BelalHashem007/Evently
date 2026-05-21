using EventBookingSystem.DomainEvents.Handlers;

namespace EventBookingSystem.DomainEvents.Dispatcher
{
    public class EventDispatcher : IEventDispatcher
    {
        private readonly IServiceProvider _serviceProvider;

        public EventDispatcher(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        public async Task PublishAsync<TEvent>(TEvent @event)
        {
            var handlers = _serviceProvider.GetServices<IEventHandler<TEvent>>();
            foreach(var handler in handlers)
            {
                await handler.HandleAsync(@event);
            }
        }
    }
}
