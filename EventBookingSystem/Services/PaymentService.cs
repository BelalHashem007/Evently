using EventBookingSystem.Common.Results;
using EventBookingSystem.Configuration;
using EventBookingSystem.Models;
using EventBookingSystem.Repositories.Interfaces;
using EventBookingSystem.Services.Interfaces;
using Stripe;
using Stripe.Checkout;

namespace EventBookingSystem.Services
{
    public class PaymentService(IConfiguration config, IUnitOfWork unitOfWork, ILogger<PaymentService> logger) : IPaymentService
    {
        public async Task<Result<Session>> GetStripeSessionAsync(int bookingId, int userId, string successUrl, string cancelUrl, CancellationToken ct = default)
        {
            //prepare stripekeys + ticket prices + quantity
            var booking = await unitOfWork.Bookings.FindOneAsync(b => b.Id == bookingId 
                                                                && b.UserId == userId
                                                                && b.Status == BookingStatus.Pending
                                                                && b.ExpiresAt >= DateTime.UtcNow, ct);
            if (booking is null)
                return Result<Session>.Failure("Booking does not exist or expired!");

            StripeKeys? stripeKeys = config.GetSection("Stripe").Get<StripeKeys>();
            if (stripeKeys is null)
                return Result<Session>.Failure("Failed to read stripe keys from configuration.");

            var bookingItems = await unitOfWork.BookingItems.FindAsync(i => i.BookingId == bookingId, ct);
            var bookingItemsTicketTypesIds = bookingItems.Select(i => i.TicketTypeId);
            var tickets = (await unitOfWork.TicketTypes.FindAsync(t => bookingItemsTicketTypesIds.Contains(t.Id), ct))
                          .ToDictionary(t => t.Id);

            //start adding tickets to checkout
            var lineItems = new List<SessionLineItemOptions>();
            foreach (var item in bookingItems)
            {
                var ticket = tickets[item.TicketTypeId];
                var itemOptions = new SessionLineItemOptions()
                {
                    Quantity = item.Quantity,
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = "egp",
                        UnitAmount = (long)(ticket.Price * 100),
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = ticket.Name,
                        }
                    }
                };
                lineItems.Add(itemOptions);
            }

            //finish checkout options
            var options = new SessionCreateOptions
            {
                SuccessUrl = successUrl,
                CancelUrl = cancelUrl,
                Mode = "payment",
                LineItems = lineItems,
                Metadata = new Dictionary<string, string> { {"bookingId", bookingId.ToString() } },
                
            };
            var requestOptions = new RequestOptions
            {
                IdempotencyKey = $"checkout-booking-{bookingId}"
            };

            //create session and log + failure if exception happens
            try
            {
                var client = new StripeClient(apiKey: stripeKeys.SecretKey);
                var service = client.V1.Checkout.Sessions;
                Session session = await service.CreateAsync(options, requestOptions, ct);
                return Result<Session>.Success(session);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to create Stripe session");
                return Result<Session>.Failure("Failed to start payment");
            }
        }

    }
}
