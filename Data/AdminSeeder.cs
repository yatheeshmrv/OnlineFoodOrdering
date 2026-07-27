using FoodOrderAPI.Models;
using Microsoft.AspNetCore.Identity;

namespace FoodOrderAPI.Data
{
    public static class AdminSeeder
    {
        public static async Task SeedAdminAsync(
            IServiceProvider serviceProvider,
            IConfiguration configuration)
        {
            var userManager =
                serviceProvider.GetRequiredService<
                    UserManager<ApplicationUser>>();

            var adminEmail = configuration["AdminUser:Email"]
                ?? throw new InvalidOperationException(
                    "Admin email is missing.");

            var adminPassword = configuration["AdminUser:Password"]
                ?? throw new InvalidOperationException(
                    "Admin password is missing.");

            var adminFullName =
                configuration["AdminUser:FullName"]
                ?? "System Administrator";

            var adminPhoneNumber =
                configuration["AdminUser:PhoneNumber"];

            var adminUser =
                await userManager.FindByEmailAsync(adminEmail);

            // Create the admin user only if it does not already exist.
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = adminFullName,
                    PhoneNumber = adminPhoneNumber,
                    EmailConfirmed = true
                };

                var createResult =
                    await userManager.CreateAsync(
                        adminUser,
                        adminPassword);

                if (!createResult.Succeeded)
                {
                    var errors = string.Join(
                        ", ",
                        createResult.Errors.Select(
                            error => error.Description));

                    throw new InvalidOperationException(
                        $"Admin creation failed: {errors}");
                }
            }

            // Assign the Admin role if it is not already assigned.
            if (!await userManager.IsInRoleAsync(
                    adminUser,
                    "Admin"))
            {
                var roleResult =
                    await userManager.AddToRoleAsync(
                        adminUser,
                        "Admin");

                if (!roleResult.Succeeded)
                {
                    var errors = string.Join(
                        ", ",
                        roleResult.Errors.Select(
                            error => error.Description));

                    throw new InvalidOperationException(
                        $"Admin role assignment failed: {errors}");
                }
            }
        }
    }
}