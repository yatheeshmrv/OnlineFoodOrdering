using Microsoft.AspNetCore.Mvc;
using FoodOrderAPI.Services;
using FoodOrderAPI.DTOs;

namespace FoodOrderAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FoodItemsController : ControllerBase
    {
        // CHANGE 1:
        // Earlier, foodItems list was inside controller.
        // Now, all food item logic is moved to FoodItemService.
        private readonly IFoodItemService _foodItemService; // Dependency Injection using constructor injection

        public FoodItemsController(IFoodItemService foodItemService)
        {
            _foodItemService = foodItemService;
        }
        // GET: api/FoodItems
        // CHANGE 2:
        // Earlier, controller directly returned foodItems list.
        // Now, controller asks service to get all food items.
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FoodItemDto>>> GetAllFoodItems()
        {
            var foodItems = await _foodItemService.GetAllFoodItemsAsync();

            return Ok(foodItems);
        }
        // GET: api/FoodItems/1
        // CHANGE 3:
        // Earlier, searching by id was done inside controller.
        // Now, service searches the food item by id.
        [HttpGet("{id}")]
        public async Task<ActionResult<FoodItemDto>> GetFoodItemById(int id)
        {
            var foodItem = await _foodItemService.GetFoodItemByIdAsync(id);

            if (foodItem == null)
            {
                return NotFound();
            }

            return Ok(foodItem);
        }

        // POST: api/FoodItems
        // CHANGE 4:
        // Earlier, validation and adding food item were inside controller.
        // Now, service validates and adds the food item.
        [HttpPost]
        public async Task<ActionResult<FoodItemDto>> CreateFoodItem(CreateFoodItemDto createFoodItemDto)
        {
            var createdFoodItem = await _foodItemService.CreateFoodItemAsync(createFoodItemDto);

            return CreatedAtAction(
                nameof(GetFoodItemById),
                new { id = createdFoodItem.Id },
                createdFoodItem
            );
        }

        // PUT: api/FoodItems/1
        // CHANGE 5:
        // Earlier, update logic was inside controller.
        // Now, service handles update logic.
        // PUT: api/FoodItems/1
        [HttpPut("{id}")]
        public async Task<ActionResult<FoodItemDto>> UpdateFoodItem(
            int id,
            UpdateFoodItemDto updateFoodItemDto)
        {
            var updatedFoodItem = await _foodItemService.UpdateFoodItemAsync(id, updateFoodItemDto);

            if (updatedFoodItem == null)
            {
                return NotFound("Food item not found");
            }

            return Ok(updatedFoodItem);
        }

        // DELETE: api/FoodItems/1
        // CHANGE 6:
        // Earlier, delete logic was inside controller.
        // Now, service handles delete logic.
        // DELETE: api/FoodItems/1
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFoodItem(int id)
        {
            var result = await _foodItemService.DeleteFoodItemAsync(id);

            if (result == false)
            {
                return NotFound("Food item not found");
            }

            return Ok("Food item deleted successfully");
        }
    }
}