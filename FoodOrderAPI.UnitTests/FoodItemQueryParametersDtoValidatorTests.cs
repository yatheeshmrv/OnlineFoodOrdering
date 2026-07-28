using FoodOrderAPI.DTOs;
using FoodOrderAPI.Validators;
using Xunit;

namespace FoodOrderAPI.UnitTests.Validators
{
    // Contains unit tests for pagination query validation.
    public class FoodItemQueryParametersDtoValidatorTests
    {
        // Verifies that invalid page values produce validation errors.
        [Fact]
        public void Validate_WhenPageValuesAreInvalid_ReturnsValidationErrors()
        {
            // Arrange: create the validator.
            var validator =
                new FoodItemQueryParametersDtoValidator();

            // Arrange: create an invalid request.
            var queryParameters =
                new FoodItemQueryParametersDto
                {
                    PageNumber = 0,
                    PageSize = 101
                };

            // Act: execute validation.
            var validationResult =
                validator.Validate(queryParameters);

            // Assert: confirm that validation failed.
            Assert.False(validationResult.IsValid);

            // Assert: confirm that PageNumber has an error.
            Assert.Contains(
                validationResult.Errors,
                error => error.PropertyName ==
                    nameof(FoodItemQueryParametersDto.PageNumber));

            // Assert: confirm that PageSize has an error.
            Assert.Contains(
                validationResult.Errors,
                error => error.PropertyName ==
                    nameof(FoodItemQueryParametersDto.PageSize));
        }

        [Fact]
        public void Validate_WhenPageValuesAreValid_ReturnsNoValidationErrors()
        {
            // Arrange
            var validator = new FoodItemQueryParametersDtoValidator();

            var queryParameters = new FoodItemQueryParametersDto
            {
                PageNumber = 1,
                PageSize = 10
            };

            // Act
            var validationResult = validator.Validate(queryParameters);

            // Assert
            Assert.True(validationResult.IsValid);
            Assert.Empty(validationResult.Errors);
        }
    }


}