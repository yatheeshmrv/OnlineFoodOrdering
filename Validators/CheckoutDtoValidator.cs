using FluentValidation;
using FoodOrderAPI.DTOs;
using FoodOrderAPI.Models;

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

            RuleFor(checkout => checkout.PaymentMethod)
                .NotEmpty()
                .WithMessage(
                    "A payment method must be selected.")
                .Must(PaymentMethods.IsSupported)
                .WithMessage(
                    "CashOnDelivery is the only supported " +
                    "payment method.");

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