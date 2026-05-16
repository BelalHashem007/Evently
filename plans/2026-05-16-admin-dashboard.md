# Admin Dashboard

## Goal

Build an admin dashboard area for the event booking system.

The admin area should include:

- A dashboard page with stat cards for totals such as events, users, bookings, and booking revenue/volume where data is available.
- Quick actions for Create Event, View Bookings, and Manage/View Users.
- A sidebar inside the Admin area with Dashboard, Events, Bookings, and Users links.
- Read-only table pages for Events, Bookings, and Users in this first pass.

Full CRUD for bookings and users is intentionally out of scope for this iteration. Existing event create/edit/cancel behavior remains available.

## Current Status

Implemented on branch `feat/admin-dashboard`. Build passes with existing warnings.

User selected this scope:

- Dashboard + sidebar + table/list pages only.
- CRUD can come later.

Existing code findings:

- Admin area currently has only `Areas/Admin/Controllers/EventsController.cs`.
- Existing admin event functionality supports create, edit, and cancel.
- Admin views currently use `Views/Shared/_Layout.cshtml` through `Areas/Admin/Views/_ViewStart.cshtml`.
- `/Admin` maps to `HomeController.Index` by route convention, but no Admin `HomeController` exists yet.
- `AppDbContext` already exposes `Events`, `TicketTypes`, `Bookings`, `BookingItems`, `Payments`, and `Notifications`.
- `IUnitOfWork` currently exposes `Events`, `TicketTypes`, and `BookingItems`, but not `Bookings`.
- Users are Identity users through `ApplicationUser`, `UserManager<ApplicationUser>`, and Identity tables.

## Completed Tasks

- [x] Read project rules.
- [x] Read requested skills: `grill-me` and `cavemen`.
- [x] Explored existing Admin area, services, UnitOfWork, models, layout, and routing.
- [x] Confirmed iteration scope with user.
- [x] Create this plan file.
- [x] Created feature branch `feat/admin-dashboard`.
- [x] Added Admin layout with sidebar navigation.
- [x] Updated `Areas/Admin/Views/_ViewStart.cshtml` to use the Admin layout.
- [x] Added Admin dashboard controller and view.
- [x] Added dashboard service and view models.
- [x] Added dashboard stat cards and quick actions.
- [x] Added admin Events index table page.
- [x] Added admin Bookings index table page.
- [x] Added admin Users index table page.
- [x] Added services/view models for Events, Bookings, and Users list data.
- [x] Added `Bookings` to `IUnitOfWork` / `UnitOfWork`.
- [x] Registered new services in `Program.cs`.
- [x] Added Admin dashboard link to public shared layout for admin users.
- [x] Fixed public cancel modal references to the Admin view model namespace.
- [x] Ran `dotnet build`.
- [x] Checked diagnostics/lints for changed files.

## Remaining Tasks

_None for this iteration. Post-feature follow-ups (out of scope here):_

- [ ] Add pagination/search/sorting for admin tables.
- [ ] Add booking/user CRUD or management actions after deciding exact behavior.
- [ ] Add role display/editing to the Users page.

## Decisions Made

- Use a dedicated Admin layout instead of making the public layout conditionally render a sidebar. This keeps public pages clean and gives all Admin pages one consistent shell.
- Place the Admin layout under `Areas/Admin/Views/Shared/_AdminLayout.cshtml`.
- Point `Areas/Admin/Views/_ViewStart.cshtml` to the Admin layout so all Admin pages get the sidebar by default.
- Add `Areas/Admin/Controllers/HomeController.cs` as the dashboard landing controller because the existing area route defaults to `{controller=Home}/{action=Index}`.
- Keep controllers thin. Controllers call services and return views.
- Add service classes for dashboard, bookings, and users. Event list data may extend `IEventService` or use a small admin event service, depending on the cleanest implementation.
- Event admin list data extends `IEventService` because event service already owns event reads and writes.
- Booking list uses `IBookingService` with `IUnitOfWork.Bookings`, `IUnitOfWork.Events`, and Identity users to build read models.
- User list uses `IUserService` and `UserManager<ApplicationUser>`.
- Use Identity APIs (`UserManager<ApplicationUser>`) for admin user list and user counts.
- Use repository/UnitOfWork patterns where they already fit. Add UnitOfWork entities only when needed.
- Do not add private utility functions inside new controllers or services. If helper logic is needed, create helper classes under `EventBookingSystem/Helpers`.
- Keep booking and user pages read-only in this iteration.
- Keep event create/edit/cancel behavior as-is, and only add an Admin Events index/list page.

## Risks / Notes

- Existing admin `EventsController` has private helper methods. Leave them alone unless this task requires touching that code.
- Booking list needs event/user names. Current implementation avoids includes by loading related event and user dictionaries for read models.
- Users are Identity records, so user management should avoid direct table mutations unless using Identity APIs.
- Dashboard revenue/stat definitions should stay simple in this pass: count persisted bookings and sum `Booking.TotalPrice` unless payment status is introduced later.
- No database schema change is expected for this read-only dashboard/list scope.
- Full CRUD, search, pagination, sorting, role editing, booking cancellation, and payment management can be future iterations.
- `dotnet build` succeeds with pre-existing warnings in migrations and model nullable properties.

## Next Immediate Action

Review the implemented admin pages in the browser as an admin user: `/Admin`, `/Admin/Events`, `/Admin/Bookings`, and `/Admin/Users`.
