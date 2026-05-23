using EventBookingSystem.BackgroundJobs;
using EventBookingSystem.DomainEvents.Handlers;

namespace EventBookingSystem.DomainEvents.Dispatcher
{
    public class EventDispatcher : IEventDispatcher
    {
        private readonly IBackgroundTaskQueue _queue;

        public EventDispatcher(IBackgroundTaskQueue queue)
        {
            _queue = queue;
        }
        public ValueTask PublishAsync<TEvent>(TEvent @event, CancellationToken ct = default)
        {
            return _queue.QueueAsync(async (_serviceProvider, ct) =>
                {
                    var handlers = _serviceProvider.GetServices<IEventHandler<TEvent>>();
                    var logger = _serviceProvider.GetRequiredService<ILogger<EventDispatcher>>();
                    foreach (var handler in handlers)
                    {
                        try
                        {
                            await handler.HandleAsync(@event, ct);
                        }
                        catch(Exception ex)
                        {
                            logger.LogError(
                                ex,
                                "Domain event handler {Handler} failed for event {Event}",
                                handler.GetType().Name,
                                typeof(TEvent).Name);
                        }
                    }
                }, ct);
        }
    }
}
