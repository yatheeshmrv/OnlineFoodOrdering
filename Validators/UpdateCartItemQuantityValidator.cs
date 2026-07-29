using FluentValidation;
using FoodOrderAPI.DTOs;

namespace FoodOrderAPI.Validators
{
    // Validates requests that change a cart item's quantity.
    public class UpdateCartItemQuantityDtoValidator
        : AbstractValidator<UpdateCartItemQuantityDto>
    {
        public UpdateCartItemQuantityDtoValidator()
        {
            // The updated quantity must remain within
            // the supported cart and order limits.
            RuleFor(request => request.Quantity)
                .InclusiveBetween(1, 50)
                .WithMessage("Quantity must be between 1 and 50.");
        }
    }
}