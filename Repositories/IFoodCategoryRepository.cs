using FoodOrderAPI.Models;

namespace FoodOrderAPI.Repositories
{
    public interface IFoodCategoryRepository
    {
        // Gets all food categories from the database.
        Task<IEnumerable<FoodCategory>> GetAllFoodCategoriesAsync();

        // Gets one food category by id.
        Task<FoodCategory?> GetFoodCategoryByIdAsync(int id);

        // Adds a new food category.
        Task<FoodCategory> CreateFoodCategoryAsync(FoodCategory foodCategory);

        // Updates an existing food category.
        Task<FoodCategory?> UpdateFoodCategoryAsync(int id, FoodCategory foodCategory);

        // Deletes a food category by id.
        Task<bool> DeleteFoodCategoryAsync(int id);

        // Checks whether a food category with the same name already exists.
        Task<bool> FoodCategoryExistsAsync(string categoryName);
    }
}