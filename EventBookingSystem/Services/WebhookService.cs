using EventBookingSystem.Common.Results;
using EventBookingSystem.Configuration;
using EventBookingSystem.DomainEvents.Dispatcher;
using EventBookingSystem.DomainEvents.Events;
using EventBookingSystem.Models;
using EventBookingSystem.Repositories.Interfaces;
using EventBookingSystem.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Stripe;
using Stripe.Checkout;

namespace EventBookingSystem.Services
{
    public class WebhookService(
        IConfiguration config,
        IUnitOfWork unitOfWork,
        ILogger<WebhookService> logger,
        IEventDispatcher eventDispatcher,
        UserManager<ApplicationUser> userManager
        ) : IWebhookService
    {
        private const string PaymentCurrency = "egp";

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
            {
                logger.LogWarning("Stripe session {SessionId} is missing a valid booking id", session.Id);
                return Result.Success();
            }

            var existingPayment = await unitOfWork.Payments.FindOneAsync(p => p.StripeSessionId == session.Id, ct);
            if (existingPayment is not null)
                return Result.Success();

            var booking = await unitOfWork.Bookings.GetBookingWithBookingItemsById(bookingId, ct);
            if (booking is null)
            {
                logger.LogWarning("Stripe session {SessionId} references missing booking {BookingId}", session.Id, bookingId);
                return Result.Success();
            }

            if (!IsPaidAmountValid(session, booking))
            {
                logger.LogWarning(
                    "Stripe session {SessionId} amount mismatch for booking {BookingId}. Expected {ExpectedAmount} {ExpectedCurrency}, received {ReceivedAmount} {ReceivedCurrency}",
                    session.Id,
                    booking.Id,
                    ToStripeAmount(booking.TotalPrice),
                    PaymentCurrency,
                    session.AmountTotal,
                    session.Currency);
                return Result.Success();
            }

            if (booking.Status == BookingStatus.Confirmed)
                return Result.Success();

            if (booking.Status != BookingStatus.Pending && booking.Status != BookingStatus.Expired)
            {
                logger.LogWarning("Stripe session {SessionId} paid for booking {BookingId} with invalid status {Status}", session.Id, booking.Id, booking.Status);
                return Result.Success();
            }

            return await ConfirmBookingAsync(booking, session, ct);
        }

        private static bool TryGetBookingId(Session session, out int bookingId)
        {
            bookingId = default;

            return session.Metadata.TryGetValue("bookingId", out var value)
                && int.TryParse(value, out bookingId);
        }

        private static bool IsPaidAmountValid(Session session, Booking booking)
        {
            return session.AmountTotal == ToStripeAmount(booking.TotalPrice)
                && string.Equals(session.Currency, PaymentCurrency, StringComparison.OrdinalIgnoreCase);
        }

        private static long ToStripeAmount(decimal amount)
        {
            return (long)Math.Round(amount * 100, MidpointRounding.AwayFromZero);
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
                var eventItem = await unitOfWork.Events.GetByIdAsync(booking.EventId);
                var user = await userManager.FindByIdAsync(booking.UserId.ToString());
                await eventDispatcher.PublishAsync(new BookingConfirmedEvent(
                    booking.Id,
                    user.Email,
                    eventItem.Name,
                    eventItem.Date,
                    eventItem.Venue,
                    booking.BookingItems.Sum(i => i.Quantity),
                    booking.TotalPrice
                    ));
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
