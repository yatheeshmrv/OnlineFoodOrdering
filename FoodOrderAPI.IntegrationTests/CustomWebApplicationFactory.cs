using FoodOrderAPI.Data;
using FoodOrderAPI.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OnlineFoodOrdering;
using Microsoft.Extensions.Configuration;

namespace FoodOrderAPI.IntegrationTests
{
    public class CustomWebApplicationFactory
        : WebApplicationFactory<Program>
    {
        private readonly string _databaseName =
        $"OnlineFoodOrderingIntegrationTests_{Guid.NewGuid()}";
        public CustomWebApplicationFactory()
        {
            const string testJwtKey =
                "IntegrationTestJwtKey12345678901234567890";

            // Test JWT configuration.
            Environment.SetEnvironmentVariable(
                "Jwt__Key",
                testJwtKey);

            Environment.SetEnvironmentVariable(
                "Jwt__Issuer",
                "FoodOrderAPI.IntegrationTests");

            Environment.SetEnvironmentVariable(
                "Jwt__Audience",
                "FoodOrderAPI.IntegrationTests");

            Environment.SetEnvironmentVariable(
                "JwtSettings__Key",
                testJwtKey);

            Environment.SetEnvironmentVariable(
                "JwtSettings__Issuer",
                "FoodOrderAPI.IntegrationTests");

            Environment.SetEnvironmentVariable(
                "JwtSettings__Audience",
                "FoodOrderAPI.IntegrationTests");

            // Test admin configuration.
            Environment.SetEnvironmentVariable(
                "AdminUser__Email",
                "integration-admin@test.local");

            Environment.SetEnvironmentVariable(
                "AdminUser__Password",
                "IntegrationTest@12345");

            Environment.SetEnvironmentVariable(
                "AdminUser__FullName",
                "Integration Test Administrator");

            Environment.SetEnvironmentVariable(
                "AdminUser__PhoneNumber",
                "9999999999");
        }

        protected override void ConfigureWebHost(
            IWebHostBuilder builder)
        {
            // Uses a separate environment for testing.
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                // Removes the real SQL Server configuration.
                services.RemoveAll<
                    DbContextOptions<ApplicationDbContext>>();

                services.RemoveAll<
                    IDbContextOptionsConfiguration<
                        ApplicationDbContext>>();

                // Adds an isolated in-memory database.
                services.AddDbContext<ApplicationDbContext>(
                    options =>
                        options.UseInMemoryDatabase(_databaseName));

                // Creates a service scope for test data.
                using var serviceProvider =
                    services.BuildServiceProvider();

                using var scope =
                    serviceProvider.CreateScope();

                var database =
                    scope.ServiceProvider
                        .GetRequiredService<
                            ApplicationDbContext>();

                database.Database.EnsureDeleted();
                database.Database.EnsureCreated();

                // Creates a category for the integration test.
                var category = new FoodCategory
                {
                    Id = 10001,
                    CategoryName = "Healthy Meals",
                    IsActive = true
                };

                database.FoodCategories.Add(category);

                // Creates a food item for the integration test.
                database.FoodItems.Add(
                new FoodItem
                {
                    Id = 10001,
                    Name = "Paneer Fried Rice",
                    Description =
                        "Fried rice prepared with paneer",
                    Price = 180m,
                    FoodCategoryId = 10001,
                    FoodCategory = category,
                    IsAvailable = true
                });

                database.SaveChanges();
            });
        }
    }
}