using FluentValidation;
using FoodOrderAPI.DTOs;

namespace FoodOrderAPI.Validators
{
    // Validates requests that add a food item to a cart.
    public class AddCartItemDtoValidator
        : AbstractValidator<AddCartItemDto>
    {
        public AddCartItemDtoValidator()
        {
            // FoodItemId must contain a valid positive ID.
            RuleFor(request => request.FoodItemId)
                .GreaterThan(0)
                .WithMessage("Food item id must be valid.");

            // Keeps cart quantities consistent with the existing
            // order quantity limit.
            RuleFor(request => request.Quantity)
                .InclusiveBetween(1, 50)
                .WithMessage("Quantity must be between 1 and 50.");
        }
    }
}