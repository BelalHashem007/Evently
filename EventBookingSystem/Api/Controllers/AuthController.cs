using EventBookingSystem.Common;
using EventBookingSystem.Services.Interfaces;
using EventBookingSystem.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EventBookingSystem.Api.Controllers
{
    [Route("/api/auth")]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        [HttpPost("login")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login([FromForm] LoginModalRequestViewModel request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(AuthResponse.Failed(GetErrors()));
            }

            var result = await authService.LoginAsync(request.Login);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("Login.Email", "Invalid login attempt.");
                return BadRequest(AuthResponse.Failed(GetErrors()));
            }

            return Ok(AuthResponse.Success(GetSafeReturnUrl(request.ReturnUrl)));
        }

        [HttpPost("signup")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignUp([FromForm] SignUpModalRequestViewModel request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(AuthResponse.Failed(GetErrors()));
            }

            var result = await authService.SignUpAsync(request.SignUp);
            if (!result.Succeeded)
            {
                AddIdentityErrors(result.Errors, "SignUp.Email");
                return BadRequest(AuthResponse.Failed(GetErrors()));
            }

            return Ok(AuthResponse.Success(GetSafeReturnUrl(request.ReturnUrl)));
        }

        private string GetSafeReturnUrl(string? returnUrl)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return returnUrl;
            }

            return Url.Action("Index", "Home") ?? "/";
        }

        private Dictionary<string, string[]> GetErrors()
        {
            return ModelState
                .Where(entry => entry.Value?.Errors.Count > 0)
                .ToDictionary(
                    entry => entry.Key,
                    entry => entry.Value!.Errors.Select(error => error.ErrorMessage).ToArray());
        }

        private void AddIdentityErrors(IEnumerable<IdentityError> errors, string key)
        {
            foreach (var error in errors)
            {
                ModelState.AddModelError(key, error.Description);
            }
        }
    }
}
