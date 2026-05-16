# Admin Event CRUD

## Goal

Implement admin-only create, edit, and cancel/disable behavior for events.

The admin should be able to manage complete events, including their ticket types, while public users continue to see only active events on the homepage and details pages.

Admin capabilities should include:

- Event creation with at least one ticket type
- Event editing, including ticket type rows in the same form
- Event cancellation/disable without hard delete
- Homepage and details admin controls for active events only

Public behavior should include:

- Homepage listing of active events only for everyone, including admins
- Cancelled events excluded from the homepage
- Cancelled event details still reachable by direct URL with a clear cancelled alert

## Current Status

**Implemented on branch `admin-event-crud`.** Ready to merge.

### What was built

- **Data:** `Event.IsCancelled` with migration `20260514083055_AddEventIsCancelled` (`bit NOT NULL DEFAULT 0`).
- **View models:** `EventFormViewModel`, `TicketTypeFormViewModel` (with `Remove`, `HasBookingItems`), `EventCancelModalViewModel`; `EventDetailsViewModel.IsCancelled`.
- **Shared result type:** `Common/Results/Result` for service-layer success/failure (replaces event-specific write result).
- **Repository:** `IUnitOfWork.BookingItems` for ticket-type booking checks; `TryCompeleteAsync` wraps save with `Result`.
- **Service:** `IEventService` / `EventService` — create, edit, cancel; homepage filters `!IsCancelled`; edit blocked for cancelled events; ticket-type rules (min one active type, no remove with bookings, price lock after bookings).
- **Controller:** `EventsController` — public `Details`; admin `Create` / `Edit` / `Cancel` with `[Authorize(Roles = "Admin")]` and anti-forgery on POST; `RemoveValidationErrorsForRemovedTickets` before model validation.
- **Views:**
  - `Views/Events/Create.cshtml`, `Edit.cshtml`, `Details.cshtml`
  - `Views/Events/PartialViews/_EventForm.cshtml`, `_TicketTypeFormRow.cshtml`, `_EventFormScripts.cshtml`, `_CancelEventModal.cshtml`
  - `Views/Home/PartialViews/_EventCard.cshtml` — Create link on homepage; Edit/Cancel on cards via shared cancel modal
- **Validation UX:** Ticket-type fields use `asp-for` / `asp-validation-for` with `HtmlFieldPrefix` `TicketTypes[i]`; summary stays `ModelOnly`; per-field spans for ticket errors; template row for JS-added ticket types.

## Completed Tasks

- [x] Reviewed existing homepage, event card, controller, service, and model structure.
- [x] Confirmed admin create/edit/cancel scope and out-of-scope items.
- [x] Confirmed no open planning questions remain.
- [x] Feature branch `admin-event-crud` (renamed from plan branch; no new branch at merge time).
- [x] Add `IsCancelled` to `Event`.
- [x] Generate and apply EF Core migration `AddEventIsCancelled`.
- [x] Add `EventFormViewModel`, `TicketTypeFormViewModel`, and `EventCancelModalViewModel`.
- [x] Extend `IUnitOfWork` with `BookingItems` (and `TryCompeleteAsync` where used).
- [x] Extend `IEventService` and `EventService` with admin write operations.
- [x] Update public event reads to exclude cancelled events from homepage listing.
- [x] Add admin Create/Edit/Cancel actions to `EventsController`.
- [x] Add Create/Edit views and shared form partials under `Views/Events/PartialViews/`.
- [x] Replace disabled admin buttons with real links, forms, and cancel modals.
- [x] Introduce shared `Result` in `Common/Results/` for app-wide service outcomes.
- [x] Fix ticket-type validation display (field spans, removed-row ModelState cleanup).
- [x] Verify with `dotnet build`.
- [x] Check diagnostics/lints for edited files.

## Remaining Tasks

_None for this feature. Post-merge follow-ups (out of scope here):_

- [ ] Future admin dashboard for cancelled-event management.
- [ ] Image upload (if desired instead of `ImageUrl` only).

## Decisions Made

- Use the existing MVC controller/service/view-model structure and keep controllers thin.
- Add admin-only write actions to `EventsController` with `[Authorize(Roles = "Admin")]` and `[ValidateAntiForgeryToken]` on POST actions.
- Keep public `EventsController.Details(int id)` and add admin Create/Edit/Cancel actions.
- Extend `IEventService` and `EventService` for admin operations; controllers validate model state, call the service, and choose views/redirects.
- Return `Common.Results.Result` from the service for expected validation failures instead of throwing.
- Add `IsCancelled` to `Event` and generate an EF Core migration that adds `IsCancelled bit not null default 0` to the `Events` table.
- Exclude cancelled events from public homepage reads; keep cancelled event details URLs viewable with a clear cancelled alert.
- Do not add hard delete UI, controller action, or service operation for this feature; cancellation is the event removal behavior.
- Do not add cancelled-event listing/management in this feature; that belongs to a future admin dashboard.
- Add `EventFormViewModel` for Create/Edit with `Id`, `Name`, `Description`, `Date`, `Venue`, `ImageUrl`, and `TicketTypes`.
- Add `TicketTypeFormViewModel` with `Id`, `Name`, `Price`, `Quantity`, and an optional delete/remove marker if ticket type deletion is allowed in edit.
- Manage ticket type creation and editing inside the event create/edit workflow.
- Require at least one ticket type for active events.
- Keep event image editing as a plain `ImageUrl` field; image upload is out of scope unless decided otherwise.
- Show `Create New Event` on the homepage for admins and `Edit`/`Cancel` on active event cards only.
- Show `Edit` and `Cancel` on the details page for admins.
- Use a Bootstrap modal for cancel confirmation and submit a real POST form with an anti-forgery token (`_CancelEventModal` partial, reused on cards and details).
- Allow ticket type removal only when the ticket type has no booking items.
- Do not leave an active event with zero ticket types after ticket type removal.
- Lock ticket type prices once booking items exist for that ticket type; admins can add a new ticket type instead of changing a booked price.
- Treat `TicketType.Quantity` as remaining available inventory for now; quantity may be edited after bookings exist but must stay zero or greater.
- Create the event and child ticket types together on create.
- Load and update existing event fields and ticket types together on edit.
- Cancel/disable an event by setting `IsCancelled = true`; cancelled events stay in the database.
- Hide cancelled events from the homepage for admins as well during this feature.
- **Validation (implemented):** Keep `asp-validation-summary` as `ModelOnly`; show ticket-type errors via `asp-validation-for` per field. Strip `ModelState` keys for rows marked `Remove` before validating the form.

## Risks / Notes

- Hiding admin buttons is not security; controller actions must enforce `[Authorize(Roles = "Admin")]`.
- Because delete behavior is configured as `NoAction`, hard deleting an event with dependent rows can fail unless the service checks dependencies first.
- Ticket type editing becomes more sensitive after bookings exist because changing price or quantity can affect historical booking meaning.
- A full admin dashboard is intentionally out of scope for this feature.
- Image upload is intentionally out of scope unless decided otherwise.

## Next Immediate Action

Merge `admin-event-crud` into the target branch after review.
