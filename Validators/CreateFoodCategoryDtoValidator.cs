using FluentValidation;
using FoodOrderAPI.DTOs;

namespace FoodOrderAPI.Validators
{
    // Contains validation rules for creating a food category.
    public class CreateFoodCategoryDtoValidator
        : AbstractValidator<CreateFoodCategoryDto>
    {
        // Configures validation rules for CreateFoodCategoryDto.
        public CreateFoodCategoryDtoValidator()
        {
            // Ensures the category name is provided before
            // checking its maximum permitted length.
            RuleFor(category => category.CategoryName)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Category name is required.")
                .MaximumLength(100)
                .WithMessage(
                    "Category name cannot exceed 100 characters.");

            // IsActive needs no validation because bool values
            // can only be true or false.
        }
    }
}