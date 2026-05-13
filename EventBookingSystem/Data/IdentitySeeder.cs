using EventBookingSystem.Exceptions;
using EventBookingSystem.Models;
using Microsoft.AspNetCore.Identity;

namespace EventBookingSystem.Data
{
    public class IdentitySeeder(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole<int>> roleManager,
        IConfiguration configuration)
    {
        private const string AdminRole = "Admin";

        public async Task SeedAdminAccountAsync()
        {
            var adminEmail = configuration["AdminSeed:Email"];
            var adminPassword = configuration["AdminSeed:Password"];

            if (string.IsNullOrWhiteSpace(adminEmail) && string.IsNullOrWhiteSpace(adminPassword))
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
            {
                throw new AdminSeedException("Admin seed requires both AdminSeed:Email and AdminSeed:Password.");
            }

            if (!await roleManager.RoleExistsAsync(AdminRole))
            {
                throw new AdminSeedException($"Required role '{AdminRole}' does not exist. Apply the role seed migration before starting the app.");
            }

            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail
                };

                var createResult = await userManager.CreateAsync(adminUser, adminPassword);
                if (!createResult.Succeeded)
                {
                    throw new AdminSeedException($"Admin seed failed: {FormatErrors(createResult.Errors)}");
                }
            }

            if (!await userManager.IsInRoleAsync(adminUser, AdminRole))
            {
                var roleResult = await userManager.AddToRoleAsync(adminUser, AdminRole);
                if (!roleResult.Succeeded)
                {
                    throw new AdminSeedException($"Admin role assignment failed: {FormatErrors(roleResult.Errors)}");
                }
            }
        }

        private static string FormatErrors(IEnumerable<IdentityError> errors)
        {
            return string.Join(" ", errors.Select(error => error.Description));
        }
    }
}
