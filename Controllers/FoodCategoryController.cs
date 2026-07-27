using FluentValidation;
using FoodOrderAPI.DTOs;
using FoodOrderAPI.Models;
using FoodOrderAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodOrderAPI.Controllers
{
    // Identifies this class as an API controller.
    [ApiController]

    // Sets the base URL as api/FoodCategory.
    [Route("api/[controller]")]
    public class FoodCategoryController : ControllerBase
    {
        // Performs food-category business operations.
        private readonly IFoodCategoryService
            _foodCategoryService;

        // Validates requests for creating food categories.
        private readonly IValidator<CreateFoodCategoryDto>
            _createFoodCategoryValidator;

        // Validates requests for updating food categories.
        private readonly IValidator<UpdateFoodCategoryDto>
            _updateFoodCategoryValidator;

        // Receives the service and validators
        // through constructor dependency injection.
        public FoodCategoryController(
            IFoodCategoryService foodCategoryService,
            IValidator<CreateFoodCategoryDto>
                createFoodCategoryValidator,
            IValidator<UpdateFoodCategoryDto>
                updateFoodCategoryValidator)
        {
            // Stores the injected food-category service.
            _foodCategoryService =
                foodCategoryService;

            // Stores the injected create-category validator.
            _createFoodCategoryValidator =
                createFoodCategoryValidator;

            // Stores the injected update-category validator.
            _updateFoodCategoryValidator =
                updateFoodCategoryValidator;
        }

        // ---------------------------------------------------------
        // PUBLIC ENDPOINTS
        // ---------------------------------------------------------

        // GET: api/FoodCategory
        // Allows anyone to view all food categories.
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<List<FoodCategory>>>
            GetAllFoodCategories()
        {
            // Retrieves all food categories through the service.
            var foodCategories =
                await _foodCategoryService
                    .GetAllFoodCategoriesAsync();

            // Returns HTTP 200 with all food categories.
            return Ok(foodCategories);
        }

        // GET: api/FoodCategory/1
        // Allows anyone to view a food category by ID.
        [AllowAnonymous]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<FoodCategory>>
            GetFoodCategoryById(int id)
        {
            // Retrieves the requested category through the service.
            var foodCategory =
                await _foodCategoryService
                    .GetFoodCategoryByIdAsync(id);

            // Returns HTTP 404 if the category does not exist.
            if (foodCategory == null)
            {
                return NotFound(new
                {
                    message = "Food category not found."
                });
            }

            // Returns HTTP 200 with the requested category.
            return Ok(foodCategory);
        }

        // ---------------------------------------------------------
        // ADMIN-ONLY ENDPOINTS
        // ---------------------------------------------------------

        // POST: api/FoodCategory
        // Allows only an Admin to create a food category.
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<FoodCategory>>
            CreateFoodCategory(
                [FromBody]
                CreateFoodCategoryDto createFoodCategoryDto)
        {
            // Executes FluentValidation rules
            // for the create-category request.
            var validationResult =
                await _createFoodCategoryValidator
                    .ValidateAsync(createFoodCategoryDto);

            // Returns HTTP 400 if validation fails.
            if (!validationResult.IsValid)
            {
                // Groups validation errors by property name.
                var errors = validationResult.Errors
                    .GroupBy(error => error.PropertyName)
                    .ToDictionary(
                        group => group.Key,
                        group => group
                            .Select(error => error.ErrorMessage)
                            .ToArray());

                return BadRequest(new
                {
                    message = "Validation failed.",
                    errors
                });
            }

            // Converts the validated DTO into
            // the FoodCategory database model.
            var foodCategory = new FoodCategory
            {
                // Removes unnecessary spaces from the name.
                CategoryName =
                    createFoodCategoryDto.CategoryName.Trim(),

                // Sets the active status received from the request.
                IsActive =
                    createFoodCategoryDto.IsActive
            };

            try
            {
                // Sends the mapped category to the service.
                var createdFoodCategory =
                    await _foodCategoryService
                        .CreateFoodCategoryAsync(foodCategory);

                // Returns HTTP 201 with the created category.
                return CreatedAtAction(
                    nameof(GetFoodCategoryById),
                    new { id = createdFoodCategory.Id },
                    createdFoodCategory);
            }
            catch (ArgumentException ex)
            {
                // Returns business-rule validation errors.
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

        // PUT: api/FoodCategory/1
        // Allows only an Admin to update a food category.
        [Authorize(Roles = "Admin")]
        [HttpPut("{id:int}")]
        public async Task<ActionResult<FoodCategory>>
            UpdateFoodCategory(
                int id,
                [FromBody]
                UpdateFoodCategoryDto updateFoodCategoryDto)
        {
            // Executes FluentValidation rules
            // for the update-category request.
            var validationResult =
                await _updateFoodCategoryValidator
                    .ValidateAsync(updateFoodCategoryDto);

            // Returns HTTP 400 if validation fails.
            if (!validationResult.IsValid)
            {
                // Groups validation errors by property name.
                var errors = validationResult.Errors
                    .GroupBy(error => error.PropertyName)
                    .ToDictionary(
                        group => group.Key,
                        group => group
                            .Select(error => error.ErrorMessage)
                            .ToArray());

                return BadRequest(new
                {
                    message = "Validation failed.",
                    errors
                });
            }

            // Converts the validated DTO into
            // the FoodCategory database model.
            var foodCategory = new FoodCategory
            {
                // Removes unnecessary spaces from the name.
                CategoryName =
                    updateFoodCategoryDto.CategoryName.Trim(),

                // Sets the updated active status.
                IsActive =
                    updateFoodCategoryDto.IsActive
            };

            try
            {
                // Sends the category ID and mapped model
                // to the service for updating.
                var updatedFoodCategory =
                    await _foodCategoryService
                        .UpdateFoodCategoryAsync(
                            id,
                            foodCategory);

                // Returns HTTP 404 if the category does not exist.
                if (updatedFoodCategory == null)
                {
                    return NotFound(new
                    {
                        message = "Food category not found."
                    });
                }

                // Returns HTTP 200 with the updated category.
                return Ok(updatedFoodCategory);
            }
            catch (ArgumentException ex)
            {
                // Returns business-rule validation errors.
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

        // DELETE: api/FoodCategory/1
        // Allows only an Admin to delete a food category.
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult>
            DeleteFoodCategory(int id)
        {
            // Attempts to delete the requested category.
            var isDeleted =
                await _foodCategoryService
                    .DeleteFoodCategoryAsync(id);

            // Returns HTTP 404 if the category does not exist.
            if (!isDeleted)
            {
                return NotFound(new
                {
                    message = "Food category not found."
                });
            }

            // Returns HTTP 204 after successful deletion.
            return NoContent();
        }
    }
}