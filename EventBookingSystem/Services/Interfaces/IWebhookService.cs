using EventBookingSystem.Common.Results;

namespace EventBookingSystem.Services.Interfaces
{
    public interface IWebhookService
    {
        public Task<Result> UpdateBooking(string body, string signatureHeader, CancellationToken ct);
    }
}
