using EventBookingSystem.Common.Results;
using Stripe.Checkout;

namespace EventBookingSystem.Services.Interfaces
{
    public interface IPaymentService
    {
        public Task<Result<Session>> GetStripeSessionAsync(int bookingId, string sucessUrl, string cancelUrl);
    }
}
