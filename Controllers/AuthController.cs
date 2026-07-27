using FluentValidation;
using FoodOrderAPI.DTOs;
using FoodOrderAPI.Models;
using FoodOrderAPI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FoodOrderAPI.Controllers
{
    // Enables API controller behaviour.
    [ApiController]

    // Sets the controller route as api/Auth.
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        // Handles user creation, lookup, password verification
        // and other ASP.NET Core Identity operations.
        private readonly UserManager<ApplicationUser> _userManager;

        // Generates JWT tokens after successful login.
        private readonly ITokenService _tokenService;

        // Validates registration request data.
        private readonly IValidator<RegisterDto> _registerValidator;

        // Validates login request data.
        private readonly IValidator<LoginDto> _loginValidator;

        // Receives all required dependencies through constructor injection.
        public AuthController(
            UserManager<ApplicationUser> userManager,
            ITokenService tokenService,
            IValidator<RegisterDto> registerValidator,
            IValidator<LoginDto> loginValidator)
        {
            // Stores the injected UserManager.
            _userManager = userManager;

            // Stores the injected token service.
            _tokenService = tokenService;

            // Stores the injected registration validator.
            _registerValidator = registerValidator;

            // Stores the injected login validator.
            _loginValidator = loginValidator;
        }

        // POST: api/Auth/register
        // Creates a new customer account.
        [HttpPost("register")]
        public async Task<IActionResult> Register(
            RegisterDto registerDto)
        {
            // Executes the FluentValidation registration rules.
            var validationResult =
                await _registerValidator.ValidateAsync(registerDto);

            // Returns HTTP 400 when any registration value is invalid.
            if (!validationResult.IsValid)
            {
                // Groups validation errors by property name.
                var errors = validationResult.Errors
                    .GroupBy(error => error.PropertyName)
                    .ToDictionary(
                        group => group.Key,
                        group => group
                            .Select(error => error.ErrorMessage)
                            .ToArray());

                return BadRequest(new
                {
                    message = "Validation failed.",
                    errors
                });
            }

            // Removes unnecessary spaces from the email.
            var email = registerDto.Email.Trim();

            // Checks whether an account already uses this email.
            var existingUser =
                await _userManager.FindByEmailAsync(email);

            // Prevents duplicate accounts.
            if (existingUser != null)
            {
                return BadRequest(new
                {
                    message =
                        "A user with this email already exists."
                });
            }

            // Creates an Identity user using validated data.
            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FullName = registerDto.FullName.Trim(),
                PhoneNumber = registerDto.PhoneNumber.Trim()
            };

            // Creates the user and securely hashes the password.
            var createResult = await _userManager.CreateAsync(
                user,
                registerDto.Password);

            // Returns Identity errors when account creation fails.
            if (!createResult.Succeeded)
            {
                return BadRequest(new
                {
                    message = "User registration failed.",
                    errors = createResult.Errors.Select(
                        error => error.Description)
                });
            }

            // Assigns the Customer role to every new user.
            var roleResult = await _userManager.AddToRoleAsync(
                user,
                "Customer");

            // Handles a failure while assigning the role.
            if (!roleResult.Succeeded)
            {
                // Removes the partially created account.
                await _userManager.DeleteAsync(user);

                return BadRequest(new
                {
                    message = "Customer role assignment failed.",
                    errors = roleResult.Errors.Select(
                        error => error.Description)
                });
            }

            // Returns a successful registration response.
            return Ok(new
            {
                message = "User registered successfully.",
                role = "Customer"
            });
        }

        // POST: api/Auth/login
        // Authenticates a user and returns a JWT token.
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            // Executes the FluentValidation login rules.
            var validationResult =
                await _loginValidator.ValidateAsync(loginDto);

            // Returns HTTP 400 when the login data is invalid.
            if (!validationResult.IsValid)
            {
                // Groups validation errors by property name.
                var errors = validationResult.Errors
                    .GroupBy(error => error.PropertyName)
                    .ToDictionary(
                        group => group.Key,
                        group => group
                            .Select(error => error.ErrorMessage)
                            .ToArray());

                return BadRequest(new
                {
                    message = "Validation failed.",
                    errors
                });
            }

            // Removes unnecessary spaces from the email.
            var email = loginDto.Email.Trim();

            // Finds the registered user using their email.
            var user =
                await _userManager.FindByEmailAsync(email);

            // Returns the same message for unknown emails and
            // incorrect passwords to avoid exposing account details.
            if (user == null)
            {
                return Unauthorized(new
                {
                    message = "Invalid email or password."
                });
            }

            // Verifies the submitted password against its stored hash.
            var isPasswordValid =
                await _userManager.CheckPasswordAsync(
                    user,
                    loginDto.Password);

            // Rejects an incorrect password.
            if (!isPasswordValid)
            {
                return Unauthorized(new
                {
                    message = "Invalid email or password."
                });
            }

            // Generates a JWT for the authenticated user.
            var token =
                await _tokenService.GenerateTokenAsync(user);

            // Returns the generated JWT token.
            return Ok(new
            {
                message = "Login successful.",
                token,
                tokenType = "Bearer"
            });
        }
    }
}