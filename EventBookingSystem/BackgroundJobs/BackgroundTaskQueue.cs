using System.Threading.Channels;

namespace EventBookingSystem.BackgroundJobs
{
    public class BackgroundTaskQueue : IBackgroundTaskQueue
    {
        private readonly Channel<Func<IServiceProvider, CancellationToken, ValueTask>> _queue =
            Channel.CreateBounded<Func<IServiceProvider, CancellationToken, ValueTask>>(100);

        public ValueTask QueueAsync(Func<IServiceProvider, CancellationToken, ValueTask> workItem, CancellationToken ct = default)
        {
            return _queue.Writer.WriteAsync(workItem, ct);
        }
        public ValueTask<Func<IServiceProvider, CancellationToken, ValueTask>> DequeueAsync(CancellationToken ct)
        {
            return _queue.Reader.ReadAsync(ct);
        }

        
    }
}
