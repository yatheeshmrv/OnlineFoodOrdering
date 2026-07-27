using FluentValidation;
using FoodOrderAPI.DTOs;

namespace FoodOrderAPI.Validators
{
    // Contains validation rules for updating an order status.
    public class UpdateOrderStatusDtoValidator
        : AbstractValidator<UpdateOrderStatusDto>
    {
        // Contains every status accepted by the application.
        private static readonly HashSet<string> ValidStatuses =
            new(StringComparer.OrdinalIgnoreCase)
            {
                "Pending",
                "Confirmed",
                "Preparing",
                "Out for Delivery",
                "Cancelled",
                "Delivered"
            };

        // Configures the validation rules.
        public UpdateOrderStatusDtoValidator()
        {
            // Validates the requested order status.
            RuleFor(dto => dto.OrderStatus)

                // Stops checking after the first failed rule.
                .Cascade(CascadeMode.Stop)

                // Ensures that a status was provided.
                .NotEmpty()
                .WithMessage("Order status is required.")

                // Ensures that the provided status is supported.
                .Must(status => ValidStatuses.Contains(status))
                .WithMessage(
                    "Order status must be Pending, Confirmed, " +
                    "Preparing, Out for Delivery, Cancelled or Delivered.");
        }
    }
}