using FoodOrderAPI.DTOs;
using FoodOrderAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace FoodOrderAPI.Controllers
{
    // This tells ASP.NET Core that this class is an API Controller
    [ApiController]

    // API URL will be: api/FoodItems
    [Route("api/[controller]")]
    public class FoodItemsController : ControllerBase
    {
        // Service object is used to call business logic
        private readonly IFoodItemService _foodItemService;

        // Constructor Injection
        // ASP.NET Core automatically provides FoodItemService object
        public FoodItemsController(IFoodItemService foodItemService)
        {
            _foodItemService = foodItemService;
        }

        // GET: api/FoodItems
        // This API returns all food items
        [HttpGet]
        public async Task<ActionResult<List<FoodItemDto>>> GetAllFoodItems()
        {
            var foodItems = await _foodItemService.GetAllFoodItemsAsync();

            return Ok(foodItems);
        }

        // GET: api/FoodItems/1
        // This API returns one food item by Id
        [HttpGet("{id}")]
        public async Task<ActionResult<FoodItemDto>> GetFoodItemById(int id)
        {
            var foodItem = await _foodItemService.GetFoodItemByIdAsync(id);

            if (foodItem == null)
            {
                return NotFound("Food item not found.");
            }

            return Ok(foodItem);
        }

        // POST: api/FoodItems
        // This API adds a new food item
        [HttpPost]
        public async Task<ActionResult<FoodItemDto>> AddFoodItem(FoodItemDto foodItemDto)
        {
            try
            {
                var addedFoodItem = await _foodItemService.AddFoodItemAsync(foodItemDto);

                return CreatedAtAction(
                    nameof(GetFoodItemById),
                    new { id = addedFoodItem.Id },
                    addedFoodItem
                );
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // PUT: api/FoodItems/1
        // This API updates an existing food item
        [HttpPut("{id}")]
        public async Task<ActionResult<FoodItemDto>> UpdateFoodItem(int id, FoodItemDto foodItemDto)
        {
            try
            {
                var updatedFoodItem = await _foodItemService.UpdateFoodItemAsync(id, foodItemDto);

                if (updatedFoodItem == null)
                {
                    return NotFound("Food item not found.");
                }

                return Ok(updatedFoodItem);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // DELETE: api/FoodItems/1
        // This API deletes an existing food item
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteFoodItem(int id)
        {
            var isDeleted = await _foodItemService.DeleteFoodItemAsync(id);

            if (!isDeleted)
            {
                return NotFound("Food item not found.");
            }

            return NoContent();
        }
    }
}