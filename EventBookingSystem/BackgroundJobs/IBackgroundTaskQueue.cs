namespace EventBookingSystem.BackgroundJobs
{
    public interface IBackgroundTaskQueue
    {
        ValueTask QueueAsync(Func<IServiceProvider, CancellationToken, ValueTask> workItem, CancellationToken ct = default);
        ValueTask<Func<IServiceProvider, CancellationToken, ValueTask>> DequeueAsync(CancellationToken ct);
    }
}
