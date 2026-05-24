# Notification UX

## Goal

Improve notification dropdown UX across behavior and visual presentation: keep unread notifications highlighted until the dropdown closes, support paged loading, make notification content readable over the page background, and keep the bell trigger visually clean.

## Current Status

Notification system already supports:

- Persisting notifications from booking domain event handlers.
- Loading latest notifications into the dropdown.
- Pushing new notifications through SignalR.
- Marking all notifications as read through `PATCH /api/notification/readAll`.
- Broadcasting `UpdateReadStatus` to all SignalR connections for the current user.
- Loading notifications in pages of 10 with a Load more button.
- Marking visible unread notifications as read only after dropdown close.

Current UX issues:

- Notification dropdown header has low contrast and is hard to read.
- White notification text blends into event images/content behind the translucent dropdown.
- Dropdown should visually match a Bootstrap list-group style like the provided reference.
- Notification bell trigger should not show the dropdown arrow; only the bell icon should be visible.
- Notification dates loaded after page refresh can display several hours behind SignalR-pushed notifications because database-loaded UTC values are serialized without UTC kind.

## Completed Tasks

- [x] Reviewed current notification flow.
- [x] Decided unread items should stay highlighted until dropdown closes.
- [x] Decided Load more should append next 10 notifications.
- [x] Decided older unread items loaded by Load more should also stay highlighted until dropdown closes.
- [x] Decided backend should return an `IsEnd` flag using `take + 1` query strategy.
- [x] Add `NotificationPageViewModel` in `EventBookingSystem/ViewModels/NotificationViewModel.cs`.
- [x] Update `INotificationService.GetNotifications` to accept `skip`, `take`, and `CancellationToken`.
- [x] Update `INotificationRepository` with paged newest-first no-tracking query.
- [x] Implement repository query using `Skip(skip).Take(take + 1)`.
- [x] Build service response by trimming extra row and setting `IsEnd`.
- [x] Update `EventBookingSystem/Controllers/NotificationController.cs` to accept `skip` and `take` query params.
- [x] Clamp paging values in controller or service to avoid invalid or huge requests.
- [x] Update `EventBookingSystem/wwwroot/js/notification.js` to read paged response shape.
- [x] Track `loadedNotificationCount`, page size, and end-of-list state in client script.
- [x] Add Load more button to dropdown when `IsEnd` is false.
- [x] Fetch next page on Load more click and append results.
- [x] Hide Load more button when final page is reached.
- [x] Move read-status update from `shown.bs.dropdown` to `hidden.bs.dropdown`.
- [x] Keep unread CSS class while dropdown is open.
- [x] Clear unread highlights and badge only after read API succeeds.
- [x] Preserve highlights and badge if read API fails.
- [x] Run diagnostics on edited files.
- [x] Run compiler check; if full build is blocked by running app file lock, document that.

## Remaining Tasks

- [x] Restyle dropdown content to use a Bootstrap list-group look similar to the provided reference.
- [x] Make the dropdown menu fully opaque so white text and empty dropdown areas do not blend with page images behind it.
- [x] Improve notification header contrast so `Notifications` is clearly readable.
- [x] Render each notification as a non-clickable list-group item with title, short absolute date, and message.
- [x] Keep unread notifications visually distinct using the active/list-group highlight style.
- [x] Ensure read notifications use solid light list-group items with readable dark/muted text.
- [x] Keep Load more visually consistent with the list-group dropdown styling.
- [x] Remove the Bootstrap dropdown arrow from the notification bell trigger while keeping the bell button behavior.
- [x] Fix refreshed notification dates by treating database-loaded `CreatedDate` values as UTC before JSON serialization.
- [ ] Check the dropdown on top of image-heavy event cards and gradient backgrounds.
- [x] Run diagnostics on edited files.
- [x] Run compiler/static asset check if needed.

## Decisions Made

- Page size is 10 notifications.
- API response should return notifications plus `IsEnd`.
- End detection should use `take + 1`, not a separate count query.
- Initial request should use `skip=0&take=10`.
- Load more request should use `skip=loadedNotificationCount&take=10`.
- Read timing should be dropdown close, not dropdown open.
- All visible unread notifications stay highlighted until close, including items loaded after clicking Load more.
- SignalR `UpdateReadStatus` remains useful for syncing other tabs/devices.
- Notification list should move toward the Bootstrap list-group visual pattern shown in the user-provided reference.
- Notification rows should remain non-clickable for now because notifications do not currently include target URLs.
- Read notifications should use solid light list-group items with dark/muted text.
- Unread notifications should use the themed Bootstrap `active` list-group style.
- Dropdown menu should be fully opaque, not glass/transparent.
- Notification dates should keep the current short absolute format, such as `May 24, 6:59 PM`.
- Notification timestamps should continue to be stored in UTC; the API/view-model mapping should ensure refreshed values serialize as UTC so JavaScript displays local time correctly.
- Bell trigger should show only the bell icon, without the dropdown caret.

## Risks / Notes

- If dropdown closes after Load more click closes the Bootstrap menu accidentally, read update may trigger early; Load more button should not unintentionally close dropdown.
- If SignalR read-status broadcast arrives from another tab while dropdown is open, current tab may clear highlights before close. This is acceptable because read status was changed elsewhere.
- The time display bug is likely caused by `DateTime.UtcNow` values losing UTC kind when loaded from the database, causing JavaScript to treat refreshed values as local time.
- Full `dotnet build` may fail while app is running because `bin/Debug/net10.0` files are locked.
- Normal `dotnet build EventBookingSystem.slnx` was blocked by locked running app binaries; `dotnet build EventBookingSystem/EventBookingSystem.csproj -o .build-check` succeeded with 0 warnings/errors.

## Next Immediate Action

Implement the visual dropdown polish and time fix: update the notification partial, `notification.js`, and `site.css` so the dropdown uses an opaque list-group style and the bell trigger no longer shows a caret, then update notification date mapping so refreshed dates display the same local time as SignalR notifications.
