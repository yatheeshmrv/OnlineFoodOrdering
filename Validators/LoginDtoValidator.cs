using FluentValidation;
using FoodOrderAPI.DTOs;

namespace FoodOrderAPI.Validators
{
    // Contains validation rules for LoginDto.
    public class LoginDtoValidator : AbstractValidator<LoginDto>
    {
        // Configures validation rules for login details.
        public LoginDtoValidator()
        {
            // Ensures the email is present before checking its format.
            RuleFor(login => login.Email)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Email is required.")
                .EmailAddress()
                .WithMessage("Enter a valid email address.");

            // Ensures that the password is provided.
            RuleFor(login => login.Password)
                .NotEmpty()
                .WithMessage("Password is required.");
        }
    }
}