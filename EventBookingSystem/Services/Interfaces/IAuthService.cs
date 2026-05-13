using EventBookingSystem.ViewModels;
using Microsoft.AspNetCore.Identity;

namespace EventBookingSystem.Services.Interfaces
{
    public interface IAuthService
    {
        Task<IdentityResult> SignUpAsync(SignUpViewModel viewModel);
        Task<SignInResult> LoginAsync(LoginViewModel viewModel);
        Task LogoutAsync();
    }
}
