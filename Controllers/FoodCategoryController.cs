using FoodOrderAPI.Models;
using FoodOrderAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace FoodOrderAPI.Controllers
{
    // This tells ASP.NET Core that this class is an API controller.
    [ApiController]

    // This sets the URL as: api/foodcategory
    [Route("api/[controller]")]
    public class FoodCategoryController : ControllerBase
    {
        // This field stores the food category service object.
        // Controller uses service to perform business operations.
        private readonly IFoodCategoryService _foodCategoryService;

        // Constructor injection.
        // ASP.NET Core automatically gives IFoodCategoryService object here.
        public FoodCategoryController(IFoodCategoryService foodCategoryService)
        {
            _foodCategoryService = foodCategoryService;
        }

        // GET: api/foodcategory
        // This method gets all food categories.
        [HttpGet]
        public async Task<ActionResult<List<FoodCategory>>> GetAllFoodCategories()
        {
            List<FoodCategory> foodCategories =
                await _foodCategoryService.GetAllFoodCategoriesAsync();

            return Ok(foodCategories);
        }

        // GET: api/foodcategory/1
        // This method gets one food category by id.
        [HttpGet("{id}")]
        public async Task<ActionResult<FoodCategory>> GetFoodCategoryById(int id)
        {
            FoodCategory? foodCategory =
                await _foodCategoryService.GetFoodCategoryByIdAsync(id);

            if (foodCategory == null)
            {
                return NotFound("Food category not found.");
            }

            return Ok(foodCategory);
        }

        // POST: api/foodcategory
        // This method creates a new food category.
        [HttpPost]
        public async Task<ActionResult<FoodCategory>> CreateFoodCategory(
            [FromBody] FoodCategory foodCategory)
        {
            try
            {
                FoodCategory createdFoodCategory =
                    await _foodCategoryService.CreateFoodCategoryAsync(foodCategory);

                return CreatedAtAction(
                    nameof(GetFoodCategoryById),
                    new { id = createdFoodCategory.Id },
                    createdFoodCategory
                );
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // PUT: api/foodcategory/1
        // This method updates an existing food category.
        [HttpPut("{id}")]
        public async Task<ActionResult<FoodCategory>> UpdateFoodCategory(
            int id,
            [FromBody] FoodCategory foodCategory)
        {
            try
            {
                FoodCategory? updatedFoodCategory =
                    await _foodCategoryService.UpdateFoodCategoryAsync(id, foodCategory);

                if (updatedFoodCategory == null)
                {
                    return NotFound("Food category not found.");
                }

                return Ok(updatedFoodCategory);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // DELETE: api/foodcategory/1
        // This method deletes a food category by id.
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteFoodCategory(int id)
        {
            bool isDeleted =
                await _foodCategoryService.DeleteFoodCategoryAsync(id);

            if (isDeleted == false)
            {
                return NotFound("Food category not found.");
            }

            return NoContent();
        }
    }
}