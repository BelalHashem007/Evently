namespace EventBookingSystem.BackgroundJobs
{
    public class DomainEventHostedService : BackgroundService
    {
        private readonly IBackgroundTaskQueue _queue;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<DomainEventHostedService> _logger;
        public DomainEventHostedService(
        IBackgroundTaskQueue queue,
        IServiceScopeFactory scopeFactory,
        ILogger<DomainEventHostedService> logger)
        {
            _queue = queue;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while(!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var workItem = await _queue.DequeueAsync(stoppingToken);
                    using var scope = _scopeFactory.CreateScope();
                    await workItem(scope.ServiceProvider, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                { }
                catch (Exception ex )
                {
                    _logger.LogError(ex, "Background domain event processing failed");
                }
            }
        }
    }
}
