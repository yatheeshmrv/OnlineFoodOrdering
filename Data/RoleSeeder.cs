using Microsoft.AspNetCore.Identity;

namespace FoodOrderAPI.Data
{
    public static class RoleSeeder
    {
        public static async Task SeedRolesAsync(
            IServiceProvider serviceProvider)
        {
            // Gets RoleManager from dependency injection.
            var roleManager =
                serviceProvider.GetRequiredService<
                    RoleManager<IdentityRole>>();

            // Roles required by the application.
            string[] roleNames =
            {
                "Admin",
                "Customer"
            };

            foreach (var roleName in roleNames)
            {
                // Create the role only when it does not already exist.
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    var result = await roleManager.CreateAsync(
                        new IdentityRole(roleName));

                    // Stop application startup if role creation fails.
                    if (!result.Succeeded)
                    {
                        var errors = string.Join(
                            ", ",
                            result.Errors.Select(error =>
                                error.Description));

                        throw new InvalidOperationException(
                            $"Failed to create role '{roleName}': {errors}");
                    }
                }
            }
        }
    }
}