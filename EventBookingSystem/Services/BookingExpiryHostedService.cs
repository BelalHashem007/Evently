using EventBookingSystem.Services.Interfaces;

namespace EventBookingSystem.Services
{
    public sealed class BookingExpiryHostedService(
        IServiceScopeFactory scopeFactory,
        ILogger<BookingExpiryHostedService> logger) : BackgroundService
    {
        private static readonly TimeSpan SweepInterval = TimeSpan.FromMinutes(15);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await ExpirePendingBookingsAsync(stoppingToken);

            using var timer = new PeriodicTimer(SweepInterval);
            try
            {
                while (await timer.WaitForNextTickAsync(stoppingToken))
                {
                    await ExpirePendingBookingsAsync(stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
            }
        }

        private async Task ExpirePendingBookingsAsync(CancellationToken ct)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
                await bookingService.ExpirePendingBookingsAsync(ct);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to expire pending bookings.");
            }
        }
    }
}
