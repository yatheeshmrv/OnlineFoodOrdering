using FluentValidation;
using FoodOrderAPI.DTOs;

namespace FoodOrderAPI.Validators
{
    // Validates search, category-filter and pagination
    // values received by the food-items GET endpoint.
    public class FoodItemQueryParametersDtoValidator
        : AbstractValidator<FoodItemQueryParametersDto>
    {
        // Configures the query-parameter validation rules.
        public FoodItemQueryParametersDtoValidator()
        {
            // Limits the optional search text.
            RuleFor(query => query.Search)
                .MaximumLength(100)
                .WithMessage(
                    "Search text cannot exceed 100 characters.")
                .When(query =>
                    !string.IsNullOrWhiteSpace(
                        query.Search));

            // Ensures the optional category ID is positive.
            RuleFor(query => query.CategoryId)
                .GreaterThan(0)
                .WithMessage(
                    "Category ID must be greater than 0.")
                .When(query =>
                    query.CategoryId.HasValue);

            // Pagination starts from page 1.
            RuleFor(query => query.PageNumber)
                .GreaterThan(0)
                .WithMessage(
                    "Page number must be greater than 0.");

            // Prevents invalid or excessively large pages.
            RuleFor(query => query.PageSize)
                .InclusiveBetween(1, 100)
                .WithMessage(
                    "Page size must be between 1 and 100.");
        }
    }
}