# Payment System Polish Plan

## Goal

Complete the current Stripe payment MVP by fixing five focused gaps:

- Handle paid webhooks that arrive after a booking locally expires.
- Verify Stripe amount and currency before confirming a booking.
- Build Stripe Checkout line items from the booking snapshot instead of mutable ticket prices.
- Improve the post-payment UI with a spinner and short client-side polling.
- Add a pending-only cancel booking button in the payment card.

This plan is saved under the repository `plans/` folder and should be treated as the source-of-truth plan for this work.

## Current Status

Implementation is complete on branch `fix/payment-polish`:

- `EventBookingSystem/Services/PaymentService.cs` creates Stripe Checkout sessions from stored booking snapshot totals and includes metadata for expected amount/currency.
- `EventBookingSystem/Services/WebhookService.cs` handles `checkout.session.completed`, verifies paid amount/currency, confirms verified `Pending` or `Expired` bookings, and ignores permanent invalid business cases after logging them.
- `EventBookingSystem/Controllers/PaymentsController.cs` marks successful Stripe returns with `TempData["PollPaymentStatus"]` so the details page can show the processing state.
- `EventBookingSystem/Controllers/BookingsController.cs` exposes authenticated `Status` and `Cancel` actions for polling and pending-only cancellation.
- `EventBookingSystem/Services/BookingService.cs` supports current-user booking status lookup and pending-only cancellation.
- `EventBookingSystem/Views/Bookings/Details.cshtml` shows Stripe processing UI, hides payment/cancel buttons during polling, and adds a pending-only cancel button.
- `EventBookingSystem/Views/Bookings/Scripts/_PaymentStatusPollingScripts.cshtml` owns the client-side polling JavaScript inside a dedicated scripts partial.
- `dotnet build` passes with existing unrelated nullable warnings in model/config classes.

Old behavior that was fixed:

```csharp
if (booking.Status != BookingStatus.Pending || booking.ExpiresAt < DateTime.UtcNow)
    return Result.Failure("Booking is not pending or expired.");
```

The webhook no longer rejects a verified paid session just because the local booking status has become `Expired`.

## Completed Tasks

- [x] Identified the paid-but-expired failure mode.
- [x] Confirmed the project already has unique `StripeSessionId` idempotency.
- [x] Confirmed booking item totals are stored as snapshots in `BookingItem.TotalPrice`.
- [x] Chose to recover verified paid bookings even if locally expired.
- [x] Chose client-side polling with a lightweight authorized status endpoint for post-payment UX.
- [x] Chose to allow user cancellation only while a booking is `Pending`.
- [x] Saved this plan under the repository `plans/` folder.
- [x] Implemented the payment polish changes on branch `fix/payment-polish`.
- [x] Verified the project with `dotnet build`.
- [x] Create a new local branch before implementation, for example `fix/payment-polish`.
- [x] Update `EventBookingSystem/Services/PaymentService.cs` so Checkout line items use `BookingItem.TotalPrice / Quantity` rather than current `TicketType.Price`.
- [x] Include enough Stripe session data to validate webhook payment totals, using expected currency `egp` and expected amount from `Booking.TotalPrice`.
- [x] Update `EventBookingSystem/Services/WebhookService.cs` to verify `AmountTotal` and `Currency` before confirming.
- [x] Update webhook booking status handling so verified paid sessions can confirm `Pending` or `Expired` bookings, but still reject `Cancelled` or already-invalid bookings.
- [x] Treat non-retryable business mismatches carefully in webhook responses: log them clearly and avoid unnecessary Stripe retry loops where appropriate.
- [x] Add an authenticated booking payment/status endpoint, likely in `EventBookingSystem/Controllers/BookingsController.cs`, that returns the current user’s booking status for polling.
- [x] Add a dedicated booking scripts partial view under `EventBookingSystem/Views/Bookings/Scripts/`, for example `_PaymentStatusPollingScripts.cshtml`, with the polling JavaScript inside a `<script>` tag.
- [x] Update `EventBookingSystem/Views/Bookings/Details.cshtml` to render that scripts partial only when the post-Stripe success polling state is needed, provide the needed `data-*` attributes or endpoint URL, show a spinner/processing state after Stripe success, poll the status endpoint about 3 times, refresh when confirmed, and show “still processing” if not confirmed yet.
- [x] Add a pending-only cancel booking flow, likely through `EventBookingSystem/Controllers/BookingsController.cs` and `EventBookingSystem/Services/BookingService.cs`, using an authenticated POST with antiforgery protection.
- [x] Add a “Cancel booking” button inside the payment card in `EventBookingSystem/Views/Bookings/Details.cshtml`, shown only for `Pending` bookings, that sets the booking status to `Cancelled` and returns to the booking details page with a clear message.
- [x] Verify with build/tests after implementation.

## Remaining Tasks

## Decisions Made

- A verified Stripe payment should confirm the booking even if the local booking status has become `Expired`.
- `Cancelled` bookings should not be auto-confirmed by payment webhook.
- Checkout pricing should use the booking snapshot in `BookingItem.TotalPrice`, not current `TicketType.Price`.
- Amount verification should compare Stripe `AmountTotal` to `Booking.TotalPrice * 100` and currency to `egp`.
- Post-payment UX should use client-side polling, not a server-side loop.
- Polling JavaScript should live in a dedicated scripts partial view under `EventBookingSystem/Views/Bookings/Scripts/` and be rendered by `Details.cshtml` only when needed, matching the existing event form scripts partial pattern.
- No new database booking status is planned for this polish pass.
- No new payment attempt/session table is planned for this polish pass.
- Users can cancel only their own `Pending` bookings from the booking details payment card.
- Cancellation should set `Booking.Status` to `BookingStatus.Cancelled`; it should not delete booking or booking item records.
- Permanent webhook business mismatches, such as missing booking metadata, missing local booking, amount mismatch, or cancelled booking, should be logged and acknowledged so Stripe does not retry events that cannot become valid through retry.
- The payment and cancel buttons should be hidden while the post-Stripe polling state is active to avoid duplicate checkout or cancellation during confirmation.

## Risks / Notes

- Confirming paid-but-expired bookings can theoretically overbook if another user bought the released tickets after expiry. This is accepted for now because the chosen product priority is avoiding “user charged but booking not confirmed.”
- Stripe Checkout session expiration may not perfectly match the app’s 15-minute booking expiry because Stripe has its own constraints; implementation should check Stripe.net support before relying on exact expiry alignment.
- If amount/currency mismatch occurs, the booking should not be confirmed. The log message should include booking id and Stripe session id for manual investigation.
- The polling endpoint must authorize by current user id so users cannot inspect another user’s booking status.
- The cancel action must authorize by current user id and reject confirmed bookings so paid bookings cannot be cancelled through this button.
- Manual browser testing is still useful for the Stripe redirect/webhook timing because `dotnet build` only verifies compile-time correctness.

## Next Immediate Action

Manually test the Stripe checkout success flow and the pending booking cancel button in the browser.
