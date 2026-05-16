# Events Homepage And Details

## Goal

Show existing events on the homepage for all users, with admin-only placeholder controls, and add a dedicated details page for each event.

Homepage event cards should show:

- Event image
- Event name
- Event date
- Truncated venue
- Price range

Admin-only homepage controls should show:

- Disabled `Create New Event` button above the event list
- Disabled `Edit` button on each event card
- Disabled `Delete` button on each event card

Event details pages should show:

- Event image at the top
- Venue card
- Ticket type cards with ticket name, price, and amount left
- About event section with the event description

## Current Status

Implementation is complete, committed, merged into `master`, and the feature branch has been deleted.

Existing findings:

- `EventBookingSystem/Models/Event.cs` already has `Name`, `Description`, `Date`, `Venue`, and `ImageUrl`.
- `EventBookingSystem/Models/TicketType.cs` already has `Name`, `Price`, `Quantity`, and `EventId`.
- `EventBookingSystem/Controllers/HomeController.cs` now passes homepage event card view models to the view.
- `EventBookingSystem/Views/Home/Index.cshtml` now renders event cards for all users.
- `EventBookingSystem/Controllers/EventsController.cs` now provides `Details(int id)`.
- `EventBookingSystem/Views/Events/Details.cshtml` now renders event details, venue, ticket types, and description.
- `IUnitOfWork` now exposes both `Events` and `TicketTypes`.
- Existing database already contains two events with three ticket types each.

## Completed Tasks

- [x] Reviewed the event and ticket type models.
- [x] Reviewed the homepage controller and view.
- [x] Reviewed the existing UnitOfWork and repository pattern.
- [x] Confirmed event data already exists in the database, so no event seeding is needed.
- [x] Confirmed ticket type cards should not include images.
- [x] Confirmed the event details page should use the event image at the top.
- [x] Confirmed event details should use `EventsController.Details(id)`.
- [x] Confirmed controllers should stay thin and call a service.
- [x] Confirmed the service should talk to UnitOfWork.
- [x] Confirmed view models should be added where needed.
- [x] Confirmed admin placeholder buttons should be disabled.
- [x] Created local feature branch `feat/events-home-details`.
- [x] Extended `IUnitOfWork` and `UnitOfWork` to expose `TicketTypes`.
- [x] Added event view models for homepage cards, event details, and ticket type cards.
- [x] Added `IEventService`.
- [x] Added `EventService` that reads events and ticket types through UnitOfWork.
- [x] Registered `IEventService` in `Program.cs`.
- [x] Updated `HomeController.Index` to call the event service and pass events to the homepage view.
- [x] Added `EventsController.Details(int id)`.
- [x] Replaced the default homepage with event cards.
- [x] Added `Views/Events/Details.cshtml`.
- [x] Handled missing event images gracefully in views.
- [x] Verified with `dotnet build`.
- [x] Checked diagnostics/lints for edited files.
- [x] Committed with Conventional Commits message `feat: show events on homepage and details page`.
- [x] Merged `feat/events-home-details` into `master`.
- [x] Deleted the feature branch after merge.

## Remaining Tasks

- None for this plan.

## Decisions Made

- Use the existing MVC structure.
- Keep controllers thin.
- Add an event service layer between controllers and repositories.
- Use the existing UnitOfWork/repository pattern instead of injecting `AppDbContext` directly into controllers.
- Expose `TicketTypes` through UnitOfWork.
- Use view models for event list and event details pages.
- Use the existing `User.IsInRole("Admin")` Razor pattern for admin-only UI.
- Admin Create/Edit/Delete controls are UI placeholders only and should be disabled.
- Do not add event or ticket type seed data.
- Do not add a ticket type image column.
- Do not implement create, edit, or delete behavior in this plan.

## Risks / Notes

- The project already had unrelated uncommitted changes before this work started, including `Program.cs` and repository files.
- The existing `AppDbContext` DbSet properties are not public, but `context.Set<T>()` in the repository can still query entities.
- Price ranges depend on ticket types existing for each event. If an event has no ticket types, the UI should show a clear fallback.
- Create/Edit/Delete buttons are only hidden/visible in the UI for this plan; real protected admin actions must still be implemented later.
- `dotnet build` succeeds, but still reports pre-existing warnings for NuGet package vulnerabilities and non-nullable model properties without constructor values.

## Next Immediate Action

Start the next feature plan, such as admin event create/edit/delete or ticket booking flow.
