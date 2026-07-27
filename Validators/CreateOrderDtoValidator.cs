using FluentValidation;
using FoodOrderAPI.DTOs;

namespace FoodOrderAPI.Validators
{
    // Contains validation rules for creating an order.
    public class CreateOrderDtoValidator
        : AbstractValidator<CreateOrderDto>
    {
        public CreateOrderDtoValidator()
        {
            // Requires at least one food item.
            RuleFor(order => order.Items)
                .NotEmpty()
                .WithMessage(
                    "At least one food item is required.");

            // Validates every item included in the order.
            RuleForEach(order => order.Items)
                .ChildRules(item =>
                {
                    // Food item ID must refer to a positive ID.
                    item.RuleFor(orderItem =>
                            orderItem.FoodItemId)
                        .GreaterThan(0)
                        .WithMessage(
                            "Food item ID must be greater than zero.");

                    // At least one unit must be ordered.
                    item.RuleFor(orderItem =>
                            orderItem.Quantity)
                        .GreaterThan(0)
                        .WithMessage(
                            "Quantity must be greater than zero.");
                });
        }
    }
}