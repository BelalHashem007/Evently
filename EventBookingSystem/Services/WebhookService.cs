using EventBookingSystem.Common.Results;
using EventBookingSystem.Configuration;
using EventBookingSystem.Models;
using EventBookingSystem.Repositories.Interfaces;
using EventBookingSystem.Services.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
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

                return stripeEvent.Type switch
                {
                    EventTypes.CheckoutSessionCompleted => await HandleCheckoutCompleted(stripeEvent, ct),
                    _ => Result.Success()
                };
            }
            catch (StripeException e)
            {
                logger.LogInformation(e, "Error: {Message}", e.Message);
                return Result.Failure("Stripe Error");
            }
            catch (Exception e)
            {
                logger.LogWarning(e, "internal server error while updating booking status");
                return Result.Failure("Internal server error");
            }
        }

        private async Task<Result> HandleCheckoutCompleted(Stripe.Event stripeEvent, CancellationToken ct)
        {
            if (stripeEvent.Data.Object is not Session session)
                return Result.Failure("Invalid checkout session");

            if (session.PaymentStatus != "paid")
                return Result.Success();

            if (!TryGetBookingId(session, out var bookingId))
                return Result.Failure("Invalid bookingId");

            var existingPayment = await unitOfWork.Payments.FindOneAsync(p => p.StripeSessionId == session.Id);
            if (existingPayment is not null)
                return Result.Success();

            var booking = await unitOfWork.Bookings.GetByIdAsync(bookingId);
            if (booking is null)
                return Result.Failure("Booking does not exist");

            if (booking.Status != BookingStatus.Pending || booking.ExpiresAt < DateTime.UtcNow)
                return Result.Failure("Booking is not pending or expired.");

            return await ConfirmBookingAsync(booking, session, ct);
        }

        private static bool TryGetBookingId(Session session, out int bookingId)
        {
            bookingId = default;

            return session.Metadata.TryGetValue("bookingId", out var value)
                && int.TryParse(value, out bookingId);
        }

        private async Task<Result> ConfirmBookingAsync(Booking booking, Session session, CancellationToken ct)
        {
            booking.Status = BookingStatus.Confirmed;

            await unitOfWork.Payments.AddAsync(new Payment
            {
                Booking = booking,
                PaidAt = DateTime.UtcNow,
                StripeSessionId = session.Id
            });

            try
            {
                await unitOfWork.CompleteAsync(ct);
                return Result.Success();
            }
            catch (DbUpdateException e) when (IsDuplicateStripeSessionException(e))
            {
                logger.LogInformation("Payment already processed for session {SessionId}", session.Id);
                return Result.Success();
            }
        }

        private static bool IsDuplicateStripeSessionException(DbUpdateException exception)
        {
            return exception.InnerException is SqlException sqlException
                && sqlException.Errors.Cast<SqlError>().Any(error => error.Number is 2601 or 2627)
                && sqlException.Message.Contains("IX_Payments_StripeSessionId", StringComparison.OrdinalIgnoreCase);
        }
    }
}
