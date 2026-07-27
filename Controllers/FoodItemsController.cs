using FluentValidation;
using FoodOrderAPI.DTOs;
using FoodOrderAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodOrderAPI.Controllers
{
    // Identifies this class as an API controller.
    [ApiController]

    // Sets the base URL as api/FoodItems.
    [Route("api/[controller]")]
    public class FoodItemsController : ControllerBase
    {
        // Performs food-item business operations.
        private readonly IFoodItemService _foodItemService;

        // Validates requests for creating food items.
        private readonly IValidator<CreateFoodItemDto>
            _createFoodItemValidator;

        // Validates requests for updating food items.
        private readonly IValidator<UpdateFoodItemDto>
            _updateFoodItemValidator;

        // Receives the service and validators
        // through constructor dependency injection.
        public FoodItemsController(
            IFoodItemService foodItemService,
            IValidator<CreateFoodItemDto>
                createFoodItemValidator,
            IValidator<UpdateFoodItemDto>
                updateFoodItemValidator)
        {
            // Stores the injected food-item service.
            _foodItemService = foodItemService;

            // Stores the injected create-item validator.
            _createFoodItemValidator =
                createFoodItemValidator;

            // Stores the injected update-item validator.
            _updateFoodItemValidator =
                updateFoodItemValidator;
        }

        // ---------------------------------------------------------
        // PUBLIC ENDPOINTS
        // ---------------------------------------------------------

        // GET: api/FoodItems
        // Allows anyone to view all food items.
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<List<FoodItemDto>>>
            GetAllFoodItems()
        {
            // Retrieves all food items through the service.
            var foodItems =
                await _foodItemService
                    .GetAllFoodItemsAsync();

            // Returns HTTP 200 with all food items.
            return Ok(foodItems);
        }

        // GET: api/FoodItems/1
        // Allows anyone to view a food item by ID.
        [AllowAnonymous]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<FoodItemDto>>
            GetFoodItemById(int id)
        {
            // Retrieves the requested item through the service.
            var foodItem =
                await _foodItemService
                    .GetFoodItemByIdAsync(id);

            // Returns HTTP 404 if the item does not exist.
            if (foodItem == null)
            {
                return NotFound(new
                {
                    message = "Food item not found."
                });
            }

            // Returns HTTP 200 with the requested item.
            return Ok(foodItem);
        }

        // ---------------------------------------------------------
        // ADMIN-ONLY ENDPOINTS
        // ---------------------------------------------------------

        // POST: api/FoodItems
        // Allows only an Admin to create a food item.
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<FoodItemDto>>
            AddFoodItem(
                [FromBody]
                CreateFoodItemDto createFoodItemDto)
        {
            // Executes FluentValidation rules
            // for the create-item request.
            var validationResult =
                await _createFoodItemValidator
                    .ValidateAsync(createFoodItemDto);

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

            // Converts the validated create DTO
            // into the DTO expected by the service.
            var foodItemDto = new FoodItemDto
            {
                // Removes unnecessary spaces from the name.
                Name = createFoodItemDto.Name.Trim(),

                // Removes unnecessary spaces from the description.
                Description =
                    createFoodItemDto.Description.Trim(),

                // Sets the validated price.
                Price = createFoodItemDto.Price,

                // Sets the selected category ID.
                FoodCategoryId =
                    createFoodItemDto.FoodCategoryId,

                // The category name is filled when reading the item.
                FoodCategoryName = null,

                // Sets the availability status.
                IsAvailable =
                    createFoodItemDto.IsAvailable
            };

            try
            {
                // Sends the mapped DTO to the service.
                var addedFoodItem =
                    await _foodItemService
                        .AddFoodItemAsync(foodItemDto);

                // Returns HTTP 201 with the created item.
                return CreatedAtAction(
                    nameof(GetFoodItemById),
                    new { id = addedFoodItem.Id },
                    addedFoodItem);
            }
            catch (ArgumentException ex)
            {
                // Returns business-rule errors,
                // such as an invalid category ID.
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

        // PUT: api/FoodItems/1
        // Allows only an Admin to update a food item.
        [Authorize(Roles = "Admin")]
        [HttpPut("{id:int}")]
        public async Task<ActionResult<FoodItemDto>>
            UpdateFoodItem(
                int id,
                [FromBody]
                UpdateFoodItemDto updateFoodItemDto)
        {
            // Executes FluentValidation rules
            // for the update-item request.
            var validationResult =
                await _updateFoodItemValidator
                    .ValidateAsync(updateFoodItemDto);

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

            // Converts the validated update DTO
            // into the DTO expected by the service.
            var foodItemDto = new FoodItemDto
            {
                // Removes unnecessary spaces from the name.
                Name = updateFoodItemDto.Name.Trim(),

                // Removes unnecessary spaces from the description.
                Description =
                    updateFoodItemDto.Description.Trim(),

                // Sets the validated price.
                Price = updateFoodItemDto.Price,

                // Sets the selected category ID.
                FoodCategoryId =
                    updateFoodItemDto.FoodCategoryId,

                // The category name is filled when reading the item.
                FoodCategoryName = null,

                // Sets the availability status.
                IsAvailable =
                    updateFoodItemDto.IsAvailable
            };

            try
            {
                // Updates the requested item through the service.
                var updatedFoodItem =
                    await _foodItemService
                        .UpdateFoodItemAsync(
                            id,
                            foodItemDto);

                // Returns HTTP 404 if the item does not exist.
                if (updatedFoodItem == null)
                {
                    return NotFound(new
                    {
                        message = "Food item not found."
                    });
                }

                // Returns HTTP 200 with the updated item.
                return Ok(updatedFoodItem);
            }
            catch (ArgumentException ex)
            {
                // Returns business-rule errors,
                // such as an invalid category ID.
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

        // DELETE: api/FoodItems/1
        // Allows only an Admin to delete a food item.
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult>
            DeleteFoodItem(int id)
        {
            // Attempts to delete the requested food item.
            var isDeleted =
                await _foodItemService
                    .DeleteFoodItemAsync(id);

            // Returns HTTP 404 if the item does not exist.
            if (!isDeleted)
            {
                return NotFound(new
                {
                    message = "Food item not found."
                });
            }

            // Returns HTTP 204 after successful deletion.
            return NoContent();
        }
    }
}