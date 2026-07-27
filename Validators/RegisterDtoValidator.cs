using FluentValidation;
using FoodOrderAPI.DTOs;

namespace FoodOrderAPI.Validators
{
    // Contains validation rules for user registration.
    public class RegisterDtoValidator : AbstractValidator<RegisterDto>
    {
        public RegisterDtoValidator()
        {
            // Validates the user's full name.
            RuleFor(register => register.FullName)
                .NotEmpty()
                .WithMessage("Full name is required.")
                .MaximumLength(100)
                .WithMessage(
                    "Full name cannot exceed 100 characters.");

            // Validates the user's email address.
            RuleFor(register => register.Email)
                .NotEmpty()
                .WithMessage("Email is required.")
                .EmailAddress()
                .WithMessage("Enter a valid email address.");

            // Validates the Indian mobile number.
            RuleFor(register => register.PhoneNumber)
                .NotEmpty()
                .WithMessage("Phone number is required.")
                .Matches(@"^[6-9]\d{9}$")
                .WithMessage(
                    "Enter a valid 10-digit Indian mobile number.");

            // Validates the user's password.
            RuleFor(register => register.Password)
                .NotEmpty()
                .WithMessage("Password is required.")

                .MinimumLength(6)
                .WithMessage(
                    "Password must contain at least 6 characters.")

                .MaximumLength(100)
                .WithMessage(
                    "Password cannot exceed 100 characters.")

                .Matches(@"[a-z]")
                .WithMessage(
                    "Password must contain at least one lowercase letter.")

                .Matches(@"[A-Z]")
                .WithMessage(
                    "Password must contain at least one uppercase letter.")

                .Matches(@"\d")
                .WithMessage(
                    "Password must contain at least one number.");

            // Ensures that confirm password is provided.
            RuleFor(register => register.ConfirmPassword)
                .NotEmpty()
                .WithMessage("Confirm password is required.")

                // Ensures both passwords contain the same value.
                .Equal(register => register.Password)
                .WithMessage(
                    "Password and confirm password must match.");
        }
    }
}