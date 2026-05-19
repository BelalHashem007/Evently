using EventBookingSystem.Common.Results;
using EventBookingSystem.Configuration;
using EventBookingSystem.Models;
using EventBookingSystem.Repositories.Interfaces;
using EventBookingSystem.Services.Interfaces;
using Stripe;
using Stripe.Checkout;

namespace EventBookingSystem.Services
{
    public class WebhookService(IConfiguration config, IUnitOfWork unitOfWork, ILogger<WebhookService> logger) : IWebhookService
    {
        public async Task<Result> UpdateBooking(string body, string signatureHeader, CancellationToken ct)
        {
            var stripeKeys = config.GetSection("Stripe").Get<StripeKeys>();
            if (stripeKeys is null)
                return Result.Failure("Failed to read stripeKeys");

            try
            {
                var stripeEvent = EventUtility.ConstructEvent(body, signatureHeader, stripeKeys.WebhookSecret);

                if (stripeEvent.Type == EventTypes.CheckoutSessionCompleted)
                {
                    var session = stripeEvent.Data.Object as Session;

                    if (session.PaymentStatus != "paid")
                        return Result.Success();

                    if (!session.Metadata.TryGetValue("bookingId", out var bookingIdValue))
                    {
                        logger.LogWarning("BookingId metadata missing");
                        return Result.Failure("Invalid metadata");
                    }

                    logger.LogInformation("Captured Event {EventType} for booking {BookingId}", stripeEvent.Type, bookingIdValue);

                    var existingPayment = await unitOfWork.Payments.FindOneAsync(p => p.StripeSessionId == session.Id);
                    if (existingPayment is not null)
                    {
                        logger.LogInformation( "Payment already processed for session {SessionId}",session.Id);
                        return Result.Success();
                    }

                    if (!int.TryParse(bookingIdValue, out int bookingId))
                    {
                        logger.LogWarning("Invalid bookingId metadata");
                        return Result.Failure("Invalid bookingId");
                    }

                    var booking = await unitOfWork.Bookings.GetByIdAsync(bookingId);

                    if (booking is null)
                        return Result.Failure("Booking does not exist");

                    if (booking.Status != BookingStatus.Pending || booking.ExpiresAt < DateTime.UtcNow)
                        return Result.Failure("Booking is not pending or expired.");

                    booking.Status = BookingStatus.Confirmed;

                    var payment = new Payment
                    {
                        Booking = booking,
                        PaidAt = DateTime.UtcNow,
                        StripeSessionId = session.Id
                    };

                    await unitOfWork.Payments.AddAsync(payment);

                    var dbResult = await unitOfWork.TryCompeleteAsync(ct);
                    if (!dbResult.Succeeded)
                        return Result.Failure(dbResult.ErrorMessage ?? "Db Error while processing payment");
                }
                else
                {
                    logger.LogInformation("Unhandled event type: {0}", stripeEvent.Type);
                }

                return Result.Success();
            }
            catch (StripeException e)
            {
                logger.LogInformation(e,"Error: {0}", e.Message);
                return Result.Failure("Stripe Error");
            }
            catch (Exception e)
            {
                logger.LogWarning(e, "internal server error while updating booking status");
                return Result.Failure("Internal server error");
            }
        }
    }
}
