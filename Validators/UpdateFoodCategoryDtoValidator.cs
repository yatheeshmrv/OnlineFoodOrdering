using FluentValidation;
using FoodOrderAPI.DTOs;

namespace FoodOrderAPI.Validators
{
    // Contains validation rules for updating a food category.
    public class UpdateFoodCategoryDtoValidator
        : AbstractValidator<UpdateFoodCategoryDto>
    {
        // Configures validation rules for UpdateFoodCategoryDto.
        public UpdateFoodCategoryDtoValidator()
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

            // IsActive requires no validation because bool values
            // can only be true or false.
        }
    }
}