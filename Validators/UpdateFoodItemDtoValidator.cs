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
            RuleFor(item => item.Name)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Food item name is required.")
                .MaximumLength(100)
                .WithMessage(
                    "Food item name cannot be more than 100 characters.");

            RuleFor(item => item.Description)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Description is required.")
                .MaximumLength(250)
                .WithMessage(
                    "Description cannot be more than 250 characters.");

            RuleFor(item => item.Price)
                .InclusiveBetween(1m, 10000m)
                .WithMessage(
                    "Price must be between 1 and 10000.");

            RuleFor(item => item.ImageUrl)
                .MaximumLength(500)
                .WithMessage(
                    "Image URL cannot be more than 500 characters.");

            RuleFor(item => item.FoodCategoryId)
                .GreaterThan(0)
                .WithMessage(
                    "Food category id must be valid.");

            // IsAvailable requires no validation because bool values
            // can only be true or false.
        }
    }
}