using EventBookingSystem.Common.Results;
using System.Security.Claims;

namespace EventBookingSystem.Services.Interfaces
{
    public interface IWebhookService
    {
        public Task<Result> UpdateBooking(string body, string signatureHeader, CancellationToken ct);
    }
}
