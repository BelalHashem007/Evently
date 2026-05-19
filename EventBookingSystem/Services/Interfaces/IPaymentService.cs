using EventBookingSystem.Common.Results;
using Stripe.Checkout;

namespace EventBookingSystem.Services.Interfaces
{
    public interface IPaymentService
    {
        public Task<Result<Session>> GetStripeSessionAsync(int bookingId, int userId, string successUrl, string cancelUrl, CancellationToken ct = default);
    }
}
