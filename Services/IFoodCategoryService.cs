using FoodOrderAPI.Models;

namespace FoodOrderAPI.Services
{
    public interface IFoodCategoryService
    {
        // Gets all food categories and returns them as a List.
        Task<List<FoodCategory>> GetAllFoodCategoriesAsync();

        // Gets one food category by id.
        Task<FoodCategory?> GetFoodCategoryByIdAsync(int id);

        // Creates a new food category.
        Task<FoodCategory> CreateFoodCategoryAsync(FoodCategory foodCategory);

        // Updates an existing food category.
        Task<FoodCategory?> UpdateFoodCategoryAsync(int id, FoodCategory foodCategory);

        // Deletes a food category by id.
        Task<bool> DeleteFoodCategoryAsync(int id);
    }
}