using FluentValidation;
using FoodOrderAPI.DTOs;

namespace FoodOrderAPI.Validators
{
    // Contains validation rules for updating a food item.
    public class UpdateFoodItemDtoValidator
        : AbstractValidator<UpdateFoodItemDto>
    {
        // Configures validation rules for UpdateFoodItemDto.
        public UpdateFoodItemDtoValidator()
        {
            // Ensures the food-item name is provided and
            // does not exceed 100 characters.
            RuleFor(item => item.Name)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Food item name is required.")
                .MaximumLength(100)
                .WithMessage(
                    "Food item name cannot be more than 100 characters.");

            // Ensures the description is provided and
            // does not exceed 250 characters.
            RuleFor(item => item.Description)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Description is required.")
                .MaximumLength(250)
                .WithMessage(
                    "Description cannot be more than 250 characters.");

            // Ensures the price is within the permitted range.
            RuleFor(item => item.Price)
                .InclusiveBetween(1m, 10000m)
                .WithMessage(
                    "Price must be between 1 and 10000.");

            // Ensures a valid category ID is provided.
            RuleFor(item => item.FoodCategoryId)
                .GreaterThan(0)
                .WithMessage(
                    "Food category id must be valid.");

            // IsAvailable requires no validation because bool values
            // can only be true or false.
        }
    }
}