using FluentValidation;
using FoodOrderAPI.Data;
using FoodOrderAPI.DTOs;
using FoodOrderAPI.ExceptionHandlers;
using FoodOrderAPI.Models;
using FoodOrderAPI.Repositories;
using FoodOrderAPI.Services;
using FoodOrderAPI.Validators;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace OnlineFoodOrdering
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ---------------------------------------------------------
            // CONTROLLERS
            // ---------------------------------------------------------

            // Registers API controllers in the dependency injection container.
            builder.Services.AddControllers();

            // Finds and registers all FluentValidation validators
            // in this assembly, including both cart validators.
            builder.Services
                .AddValidatorsFromAssemblyContaining<
                    CreateOrderDtoValidator>();

            // ---------------------------------------------------------
            // DATABASE CONFIGURATION
            // ---------------------------------------------------------

            // Registers ApplicationDbContext and connects it to SQL Server
            // using the DefaultConnection value from appsettings.json.
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString(
                        "DefaultConnection")));

            // ---------------------------------------------------------
            // REPOSITORY REGISTRATION
            // ---------------------------------------------------------

            // One repository instance is created for each HTTP request.
            builder.Services.AddScoped<
                IFoodCategoryRepository,
                FoodCategoryRepository>();

            builder.Services.AddScoped<
                IFoodItemRepository,
                FoodItemRepository>();

            builder.Services.AddScoped<
                IOrderRepository,
                OrderRepository>();

            // Registers shopping-cart database operations.
            builder.Services.AddScoped<
                ICartRepository,
                CartRepository>();

            // ---------------------------------------------------------
            // SERVICE REGISTRATION
            // ---------------------------------------------------------

            // Registers the application business-logic services.
            builder.Services.AddScoped<
                IFoodCategoryService,
                FoodCategoryService>();

            builder.Services.AddScoped<
                IFoodItemService,
                FoodItemService>();

            builder.Services.AddScoped<
                IOrderService,
                OrderService>();

            // Registers shopping-cart business logic.
            builder.Services.AddScoped<
                ICartService,
                CartService>();

            // Generates JWT tokens after a successful login.
            builder.Services.AddScoped<
                ITokenService,
                TokenService>();

            // Registers the FluentValidation validator for LoginDto.
            builder.Services.AddScoped<
                IValidator<LoginDto>,
                LoginDtoValidator>();

            // Registers validation rules for creating food categories.
            builder.Services.AddScoped<
                IValidator<CreateFoodCategoryDto>,
                CreateFoodCategoryDtoValidator>();

            // Registers validation rules for updating food categories.
            builder.Services.AddScoped<
                IValidator<UpdateFoodCategoryDto>,
                UpdateFoodCategoryDtoValidator>();

            // Registers validation rules for creating food items.
            builder.Services.AddScoped<
                IValidator<CreateFoodItemDto>,
                CreateFoodItemDtoValidator>();

            // Registers validation rules for updating food items.
            builder.Services.AddScoped<
                IValidator<UpdateFoodItemDto>,
                UpdateFoodItemDtoValidator>();

            // ---------------------------------------------------------
            // GLOBAL EXCEPTION HANDLING
            // ---------------------------------------------------------

            // Registers the custom global exception handler.
            builder.Services.AddExceptionHandler<
                GlobalExceptionHandler>();

            // Enables standardized ProblemDetails error responses.
            builder.Services.AddProblemDetails();

            // ---------------------------------------------------------
            // ASP.NET CORE IDENTITY
            // ---------------------------------------------------------

            builder.Services
                .AddIdentityCore<ApplicationUser>(options =>
                {
                    // Password requirements for registered users.
                    options.Password.RequiredLength = 6;
                    options.Password.RequireDigit = true;
                    options.Password.RequireLowercase = true;
                    options.Password.RequireUppercase = true;

                    // Special characters are not mandatory.
                    options.Password.RequireNonAlphanumeric = false;

                    // Prevents multiple accounts from using the same email.
                    options.User.RequireUniqueEmail = true;
                })

                // Enables role-based authorization such as Admin and Customer.
                .AddRoles<IdentityRole>()

                // Stores Identity users and roles in SQL Server.
                .AddEntityFrameworkStores<ApplicationDbContext>()

                // Adds token providers used for operations such as
                // password reset and email confirmation.
                .AddDefaultTokenProviders();

            // ---------------------------------------------------------
            // JWT CONFIGURATION VALUES
            // ---------------------------------------------------------

            // Jwt:Key should be stored in User Secrets instead of appsettings.json.
            var jwtKey = builder.Configuration["Jwt:Key"]
                ?? throw new InvalidOperationException(
                    "JWT key is missing.");

            var jwtIssuer = builder.Configuration["Jwt:Issuer"]
                ?? throw new InvalidOperationException(
                    "JWT issuer is missing.");

            var jwtAudience = builder.Configuration["Jwt:Audience"]
                ?? throw new InvalidOperationException(
                    "JWT audience is missing.");

            // ---------------------------------------------------------
            // JWT AUTHENTICATION
            // ---------------------------------------------------------

            builder.Services
                .AddAuthentication(options =>
                {
                    // JWT bearer authentication is used by default
                    // when ASP.NET Core tries to identify a user.
                    options.DefaultAuthenticateScheme =
                        JwtBearerDefaults.AuthenticationScheme;

                    // JWT bearer authentication is also used when
                    // an unauthenticated request accesses [Authorize].
                    options.DefaultChallengeScheme =
                        JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters =
                        new TokenValidationParameters
                        {
                            // Confirms that the token was created
                            // by the expected issuer.
                            ValidateIssuer = true,
                            ValidIssuer = jwtIssuer,

                            // Confirms that the token was created
                            // for the expected audience.
                            ValidateAudience = true,
                            ValidAudience = jwtAudience,

                            // Rejects expired tokens.
                            ValidateLifetime = true,

                            // Confirms that the JWT signature is valid.
                            ValidateIssuerSigningKey = true,

                            // Uses the same secret key that was used
                            // by TokenService when creating the token.
                            IssuerSigningKey =
                                new SymmetricSecurityKey(
                                    Encoding.UTF8.GetBytes(jwtKey)),

                            // The token becomes invalid immediately
                            // when its expiration time is reached.
                            ClockSkew = TimeSpan.Zero
                        };
                });

            // Enables authorization attributes such as:
            // [Authorize] and [Authorize(Roles = "Admin")].
            builder.Services.AddAuthorization();

            // ---------------------------------------------------------
            // BUILD THE APPLICATION
            // ---------------------------------------------------------

            var app = builder.Build();

            // ---------------------------------------------------------
            // HTTP REQUEST PIPELINE
            // ---------------------------------------------------------

            // Creates the Admin and Customer roles when they do not exist.
            using (var scope = app.Services.CreateScope())
            {
                // Roles must be created before assigning the Admin role.
                await RoleSeeder.SeedRolesAsync(
                    scope.ServiceProvider);

                // Creates the default admin account and assigns the Admin role.
                await AdminSeeder.SeedAdminAsync(
                    scope.ServiceProvider,
                    builder.Configuration);
            }

            // Sends unhandled exceptions to GlobalExceptionHandler.
            app.UseExceptionHandler();

            // Redirects HTTP requests to HTTPS.
            app.UseHttpsRedirection();

            // Reads and validates the JWT from the Authorization header.
            // This must come before UseAuthorization().
            app.UseAuthentication();

            // Checks whether the authenticated user is allowed
            // to access endpoints protected by [Authorize].
            app.UseAuthorization();

            // Maps controller routes such as api/Auth/login.
            app.MapControllers();

            // Starts the application.
            app.Run();
        }
    }
}