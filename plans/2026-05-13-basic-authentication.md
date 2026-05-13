# Basic Authentication

## Goal

Implement basic ASP.NET Core Identity authentication for the Event Booking System:

- Register
- Login
- Logout
- Auth-aware navbar
- Configuration-based admin account seeding

## Current Status

Implementation is complete on branch `feat/basic-authentication`. Build verification passed when outputting to a temporary folder because a running `EventBookingSystem` process is locking the normal `bin/Debug/net10.0` output files.

Existing findings:

- `[EventBookingSystem/Program.cs](../EventBookingSystem/Program.cs)` already registers ASP.NET Core Identity with `ApplicationUser` and `IdentityRole<int>`.
- `[EventBookingSystem/Controllers/AuthController.cs](../EventBookingSystem/Controllers/AuthController.cs)` currently has SignUp GET/POST only.
- `[EventBookingSystem/Services/AuthService.cs](../EventBookingSystem/Services/AuthService.cs)` currently creates users and signs them in, but does not surface Identity errors or assign roles.
- `[EventBookingSystem/Views/Shared/_Layout.cshtml](../EventBookingSystem/Views/Shared/_Layout.cshtml)` already shows Login/Sign Up for anonymous users, but does not render authenticated/admin navbar state.
- No admin controller or views currently exist.

## Completed Tasks

- [x] Reviewed current auth controller, service, signup view model, signup view, layout, Identity setup, EF context, and user model.
- [x] Confirmed admin seed credentials should come from configuration.
- [x] Confirmed login should include Email, Password, and Remember Me.
- [x] Confirmed login should use a safe local `returnUrl` when available, otherwise redirect to `Home/Index`.
- [x] Confirmed logout should redirect to `Home/Index`.
- [x] Confirmed `Admin` and `User` roles already exist in the database from an existing migration.
- [x] Confirmed seeded admin account should receive `Admin`.
- [x] Confirmed newly registered users should receive `User`.
- [x] Confirmed authenticated navbar should show email + Logout, with an Admin marker/link for Admin users.
- [x] Created local feature branch `feat/basic-authentication`.
- [x] Added `[EventBookingSystem/ViewModels/LoginViewModel.cs](../EventBookingSystem/ViewModels/LoginViewModel.cs)` with Email, Password, and Remember Me validation.
- [x] Added `[EventBookingSystem/Views/Auth/Login.cshtml](../EventBookingSystem/Views/Auth/Login.cshtml)`.
- [x] Updated `[EventBookingSystem/Services/Interfaces/IAuthService.cs](../EventBookingSystem/Services/Interfaces/IAuthService.cs)` for signup, login, and logout operations.
- [x] Updated `[EventBookingSystem/Services/AuthService.cs](../EventBookingSystem/Services/AuthService.cs)` for password login, logout, signup Identity errors, and `User` role assignment.
- [x] Updated `[EventBookingSystem/Controllers/AuthController.cs](../EventBookingSystem/Controllers/AuthController.cs)` for signup validation, login, logout, and safe local returnUrl redirects.
- [x] Updated `[EventBookingSystem/Views/Shared/_Layout.cshtml](../EventBookingSystem/Views/Shared/_Layout.cshtml)` for anonymous/authenticated/admin navbar states.
- [x] Added `[EventBookingSystem/Data/IdentitySeeder.cs](../EventBookingSystem/Data/IdentitySeeder.cs)` for config-driven admin account seeding only.
- [x] Added `[EventBookingSystem/Exceptions/AdminSeedException.cs](../EventBookingSystem/Exceptions/AdminSeedException.cs)` and replaced admin seed `InvalidOperationException` usage with the custom exception.
- [x] Updated `[EventBookingSystem/Program.cs](../EventBookingSystem/Program.cs)` to run admin account seeding at startup.
- [x] Refactored `IdentitySeeder` from a static helper to a scoped DI service using constructor injection.
- [x] Verified build with `dotnet build .\EventBookingSystem\EventBookingSystem.csproj -o "$env:TEMP\EventBookingSystemBuildVerify"`.
- [x] Checked diagnostics/lints for edited files.
- [x] Prepared Conventional Commits message suggestion: `feat: implement basic identity authentication`.

## Remaining Tasks

- [ ] After the user commits and explicitly approves the git workflow continuation, merge the feature branch back into the main development branch and delete the feature branch.

## Decisions Made

- Use ASP.NET Core Identity already configured in the project.
- Keep the current MVC controller/service/view-model structure.
- Add a dedicated login view model instead of reusing signup data.
- Use Identity cookie authentication through `SignInManager`.
- Use `RememberMe` for persistent login; default is non-persistent.
- Register successful new users into the `User` role.
- Do not seed roles in code because `Admin` and `User` already exist in the database from an existing migration.
- Seed the admin user from configuration keys:
  - `AdminSeed:Email`
  - `AdminSeed:Password`
- Do not store real admin credentials in tracked appsettings files.
- If both admin seed values are absent, skip admin user creation.
- If only one admin seed value is present, fail startup with a clear configuration error.
- Because no admin area exists yet, the Admin navbar item should be conservative: either a simple role marker or a link placeholder until an admin page is added.
- Follow the git workflow rule by creating a branch before implementation.
- Follow the commit rule by not running `git commit`; provide a Conventional Commits message suggestion for the user to apply.
- Use constructor injection for `IdentitySeeder`; `Program.cs` creates a startup scope and resolves the seeder from DI.

## Risks / Notes

- The git workflow rule asks to merge and delete the feature branch after completion. That depends on the user committing the changes first, because the separate commit rule says the agent must not run `git commit`.
- Admin navbar link target is limited because no admin controller/views exist yet.
- Admin seeding should assume the `Admin` role exists; if it does not, the seed method should fail clearly instead of creating roles silently.
- Startup seeding should avoid writing secrets to tracked files.
- Normal build output is currently locked by `EventBookingSystem (22148)`, so verification used a temporary output folder instead.
- Existing NuGet vulnerability warnings remain for transitive/packages `NuGet.Packaging` and `NuGet.Protocol` 6.12.1.

## Next Immediate Action

Review the implemented changes, set local admin seed configuration if needed, and commit using the suggested Conventional Commits message.
