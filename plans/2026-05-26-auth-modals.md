# Auth Login And Sign Up Modals

## Goal

Replace user-facing Login and Sign Up pages with Bootstrap modals. All Login/Sign Up clicks should open modals, users can close them, and validation errors should display inside the same modal.

## Current Status

- `EventBookingSystem/Views/Auth/` has been removed because Login and Sign Up now live in shared modal partials.
- `EventBookingSystem/Views/Shared/_Layout.cshtml` renders Login and Sign Up modal triggers for anonymous users.
- `EventBookingSystem/Views/Events/Details.cshtml` opens Login/Sign Up modals for anonymous booking prompts.
- `EventBookingSystem/Controllers/AuthController.cs` now only contains MVC/view-style auth behavior, currently Logout.
- `EventBookingSystem/Api/Controllers/AuthController.cs` handles JSON-only Login and Sign Up modal POSTs.
- `EventBookingSystem/wwwroot/js/auth-modals.js` submits modal forms through AJAX and renders returned validation errors without changing the browser URL.
- Bootstrap modal patterns already exist in `EventBookingSystem/Areas/Admin/Views/Events/PartialViews/_CancelEventModal.cshtml`.

## Completed Tasks

- [x] Reviewed project planning rules in `.cursor/rules/project-planning.md`.
- [x] Reviewed current auth views, layout links, event details auth links, auth controller, view models, and existing modal pattern.
- [x] Confirmed auth should be modal-based, not standalone-page UX.
- [x] Confirmed validation errors should appear in the modal.
- [x] Confirmed all public Login/Sign Up links should open modals.
- [x] Confirmed modals should include links/buttons to switch between Login and Sign Up.
- [x] Confirmed modal visuals should stay Bootstrap-clean, using current fields.
- [x] Confirmed anonymous redirects should return to the original page and auto-open the login modal.
- [x] Created local feature branch `feat/auth-modals`.
- [x] Created shared Login and Sign Up modal partials under `EventBookingSystem/Views/Shared/PartialViews/`.
- [x] Rendered anonymous auth modals from `EventBookingSystem/Views/Shared/_Layout.cshtml`.
- [x] Changed navbar Login/Sign Up controls to open Bootstrap modals.
- [x] Changed event details Login/Sign Up controls to open Bootstrap modals.
- [x] Updated anonymous booking redirect to return to event details with the login modal open.
- [x] Updated auth GET routes to redirect to a host page with the requested modal open.
- [x] Updated invalid auth POST handling to re-render Home, Privacy, or Event Details with the failed modal open and server errors preserved.
- [x] Loaded validation scripts for anonymous modal forms from the shared layout.
- [x] Ran diagnostics on edited files.
- [x] Verified build with `dotnet build .\EventBookingSystem\EventBookingSystem.csproj -o "$env:TEMP\EventBookingSystemBuildVerify"`.
- [x] Identified UX issue: failed normal POST displays the correct page content but leaves the address bar on `/Auth/Login` or `/Auth/SignUp`.
- [x] Decided to replace normal modal POST failure flow with AJAX submit before continuing implementation.
- [x] Decided to remove standalone auth views and the `Views/Auth/` folder.
- [x] Decided to remove Login/SignUp GET endpoints because direct auth pages are no longer part of the app.
- [x] Decided to split auth endpoints into MVC and API controllers.
- [x] Deleted `EventBookingSystem/Views/Auth/Login.cshtml`.
- [x] Deleted `EventBookingSystem/Views/Auth/SignUp.cshtml`.
- [x] Removed the now-empty `EventBookingSystem/Views/Auth/` folder.
- [x] Removed Login and SignUp GET endpoints from the MVC auth controller.
- [x] Kept `EventBookingSystem/Controllers/AuthController.cs` for MVC/view-style auth actions only, currently `Logout`.
- [x] Added `EventBookingSystem/Api/Controllers/AuthController.cs` for JSON Login and Sign Up POST endpoints at `/api/auth/login` and `/api/auth/signup`.
- [x] Moved modal request DTOs into `EventBookingSystem/ViewModels/LoginModalRequestViewModel.cs` and `EventBookingSystem/ViewModels/SignUpModalRequestViewModel.cs`.
- [x] Moved shared auth JSON response DTO into `EventBookingSystem/Common/AuthResponse.cs`.
- [x] Made API Login and SignUp return structured JSON only: validation errors on failure, redirect URL on success.
- [x] Kept `[ValidateAntiForgeryToken]` on API auth POSTs and send the antiforgery token through AJAX from the modal forms.
- [x] Added `EventBookingSystem/wwwroot/js/auth-modals.js`.
- [x] Intercepted Login and Sign Up modal form submits with JavaScript.
- [x] Kept client-side validation first; AJAX submit only runs when unobtrusive validation passes.
- [x] Rendered server-side validation errors back into the correct modal without changing browser URL.
- [x] Redirect browser only after successful login/signup using the JSON redirect URL.
- [x] Pointed modal forms at API endpoints instead of MVC `/Auth/Login` and `/Auth/SignUp`.
- [x] Removed host-view re-render fallback helpers from the MVC auth controller.
- [x] Updated anonymous booking redirect so it returns to the event details page with `authModal=login` directly, not through removed auth GET endpoints.
- [x] Loaded the new auth modal script from `_Layout.cshtml` for anonymous users.
- [x] Removed obsolete redirect middleware that only existed for `/auth/login` and `/auth/signup`.
- [x] Updated cookie unauthorized redirects to send users to Home with `authModal=login` and a safe return URL.
- [x] Ran diagnostics on edited files.
- [x] Verified build with `dotnet build .\EventBookingSystem\EventBookingSystem.csproj -o "$env:TEMP\EventBookingSystemBuildVerify"`.

## Remaining Tasks

- [ ] Manually test modal open/close, modal switching, invalid AJAX errors, successful login/signup redirects, and anonymous booking redirect in the browser.

## Decisions Made

- Use Bootstrap modals, matching existing project modal style.
- All public Login/Sign Up entry points should open modals, including navbar and event details prompt.
- Do not design polished custom auth UI in this task; keep current fields and Bootstrap form styling.
- Login modal should offer switching to Sign Up; Sign Up modal should offer switching to Login.
- Auth modal form submissions should use AJAX, not normal full-page POST.
- Invalid login/signup submissions should keep the modal open, render errors in the modal, and leave the browser URL unchanged.
- Successful login/signup submissions should redirect the browser to the server-provided safe local return URL.
- Anonymous booking redirects should return to the original page and auto-open login modal instead of showing a standalone login page.
- Direct auth URLs are not part of desired UX; remove Login/SignUp GET endpoints instead of keeping redirect shims.
- `Views/Auth/` should be deleted because no standalone auth pages remain.
- MVC `AuthController` should stay in `EventBookingSystem/Controllers/` for view/navigation actions such as Logout.
- JSON Login/SignUp should live in `EventBookingSystem/Api/Controllers/` so modal authentication is clearly API-only.
- Auth modal form fields use `Login` and `SignUp` prefixes so validation errors stay scoped to the correct modal.
- Failed auth POSTs should no longer re-render host views for modal validation errors.
- Server-side validation errors should be converted into a simple field/error JSON shape for the modal script.
- Auto-open modal behavior should only be query-driven for real host pages like event details, not via auth GET endpoints.

## Risks / Notes

- AJAX means JavaScript is required for modal form submission. This is acceptable because the requested UX is modal-only.
- Rendering modal forms in the shared layout still needs separate `LoginViewModel` and `SignUpViewModel` instances.
- `_ValidationScriptsPartial` currently loads only inside auth pages; moving auth forms into layout means validation scripts may need to be loaded globally or conditionally for anonymous users.
- Removing auth GET endpoints means any old direct `/Auth/Login` or `/Auth/SignUp` link will no longer be supported. Existing project links must be updated to modal triggers or host-page redirects.
- Return URL handling must remain local-only through existing `RedirectToLocal` logic.
- AJAX error rendering must map prefixed field keys like `Login.Email` and `SignUp.Password` to the matching modal validation spans.
- Cookie auth redirects now route unauthorized requests to `/?authModal=login&returnUrl=...` so removed Login GET routes are not needed for `[Authorize]` flows.
- Build still reports existing nullable warnings in configuration, notification, email, event, payment, and booking models. No new build errors were introduced.

## Next Immediate Action

Run browser verification for modal open/close, modal switching, invalid AJAX errors, successful login/signup redirects, and anonymous booking redirect.
