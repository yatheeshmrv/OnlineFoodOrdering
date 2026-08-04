using FluentValidation;
using FoodOrderAPI.DTOs;

namespace FoodOrderAPI.Validators
{
    // Validates the delivery-address information submitted
    // when a customer creates a new saved address.
    public class CreateUserAddressDtoValidator
        : AbstractValidator<CreateUserAddressDto>
    {
        public CreateUserAddressDtoValidator()
        {
            RuleFor(address => address.AddressLabel)
                .NotEmpty()
                .WithMessage("Address label is required.")
                .MaximumLength(30)
                .WithMessage(
                    "Address label cannot exceed 30 characters.");

            RuleFor(address => address.RecipientName)
                .NotEmpty()
                .WithMessage("Recipient name is required.")
                .MaximumLength(100)
                .WithMessage(
                    "Recipient name cannot exceed 100 characters.");

            RuleFor(address => address.RecipientPhone)
                .NotEmpty()
                .WithMessage("Recipient phone number is required.")
                .Matches(@"^[6-9]\d{9}$")
                .WithMessage(
                    "Recipient phone number must be a valid " +
                    "10-digit Indian mobile number.");

            RuleFor(address => address.AddressLine1)
                .NotEmpty()
                .WithMessage("Address line 1 is required.")
                .MaximumLength(200)
                .WithMessage(
                    "Address line 1 cannot exceed 200 characters.");

            RuleFor(address => address.AddressLine2)
                .MaximumLength(200)
                .WithMessage(
                    "Address line 2 cannot exceed 200 characters.")
                .When(address =>
                    !string.IsNullOrWhiteSpace(
                        address.AddressLine2));

            RuleFor(address => address.Landmark)
                .MaximumLength(150)
                .WithMessage(
                    "Landmark cannot exceed 150 characters.")
                .When(address =>
                    !string.IsNullOrWhiteSpace(address.Landmark));

            RuleFor(address => address.City)
                .NotEmpty()
                .WithMessage("City is required.")
                .MaximumLength(100)
                .WithMessage(
                    "City cannot exceed 100 characters.");

            RuleFor(address => address.State)
                .NotEmpty()
                .WithMessage("State is required.")
                .MaximumLength(100)
                .WithMessage(
                    "State cannot exceed 100 characters.");

            RuleFor(address => address.PostalCode)
                .NotEmpty()
                .WithMessage("Postal code is required.")
                .Matches(@"^[1-9]\d{5}$")
                .WithMessage(
                    "Postal code must be a valid 6-digit " +
                    "Indian PIN code.");
        }
    }
}