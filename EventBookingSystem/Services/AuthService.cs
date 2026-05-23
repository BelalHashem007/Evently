using EventBookingSystem.Models;
using EventBookingSystem.Services.Interfaces;
using EventBookingSystem.ViewModels;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace EventBookingSystem.Services
{
    public class AuthService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager) : IAuthService
    {
        public async Task<SignInResult> LoginAsync(LoginViewModel viewModel)
        {
            return await signInManager.PasswordSignInAsync(
                viewModel.Email,
                viewModel.Password,
                viewModel.RememberMe,
                lockoutOnFailure: false);
        }

        public async Task LogoutAsync()
        {
            await signInManager.SignOutAsync();
        }

        public async Task<IdentityResult> SignUpAsync(SignUpViewModel viewModel)
        {
            var user = await userManager.FindByEmailAsync(viewModel.Email);
            if (user != null)
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Description = "Email is already registered."
                });
            }

            user = new ApplicationUser
            {
                UserName = viewModel.Email,
                Email = viewModel.Email
            };

            var creationResult = await userManager.CreateAsync(user, viewModel.Password);
            if (!creationResult.Succeeded)
            {
                return creationResult;
            }

            var roleResult = await userManager.AddToRoleAsync(user, "User");
            if (!roleResult.Succeeded)
            {
                await userManager.DeleteAsync(user);
                return roleResult;
            }

            await signInManager.SignInAsync(user, isPersistent: false);
            return IdentityResult.Success;
        }
    }
}
