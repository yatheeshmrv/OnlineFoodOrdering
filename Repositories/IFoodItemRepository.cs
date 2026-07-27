using FoodOrderAPI.Models;

namespace FoodOrderAPI.Repositories
{
    public interface IFoodItemRepository
    {
        // Gets all food items from the database.
        Task<List<FoodItem>> GetAllFoodItemsAsync();

        // Gets food items using search, category filtering
        // and pagination.
        Task<(List<FoodItem> Items, int TotalCount)>
            GetPagedFoodItemsAsync(
                string? search,
                int? categoryId,
                int pageNumber,
                int pageSize);

        // Gets one food item based on its ID.
        Task<FoodItem?> GetFoodItemByIdAsync(int id);

        // Adds a new food item to the database.
        Task<FoodItem> AddFoodItemAsync(FoodItem foodItem);

        // Updates an existing food item.
        Task<FoodItem?> UpdateFoodItemAsync(
            int id,
            FoodItem foodItem);

        // Deletes an existing food item based on its ID.
        Task<bool> DeleteFoodItemAsync(int id);
    }
}