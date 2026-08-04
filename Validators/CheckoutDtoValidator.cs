using FluentValidation;
using FoodOrderAPI.DTOs;

namespace FoodOrderAPI.Validators
{
    // Validates the information submitted when a customer
    // checks out their shopping cart.
    public class CheckoutDtoValidator
        : AbstractValidator<CheckoutDto>
    {
        public CheckoutDtoValidator()
        {
            RuleFor(checkout => checkout.UserAddressId)
                .GreaterThan(0)
                .WithMessage(
                    "A valid delivery address must be selected.");

            RuleFor(checkout =>
                    checkout.DeliveryInstructions)
                .MaximumLength(500)
                .WithMessage(
                    "Delivery instructions cannot exceed " +
                    "500 characters.")
                .When(checkout =>
                    !string.IsNullOrWhiteSpace(
                        checkout.DeliveryInstructions));
        }
    }
}