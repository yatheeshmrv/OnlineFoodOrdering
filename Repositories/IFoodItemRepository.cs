using FoodOrderAPI.Models;

namespace FoodOrderAPI.Repositories
{
    public interface IFoodItemRepository
    {
        // This method gets all food items from database
        Task<List<FoodItem>> GetAllFoodItemsAsync();

        // This method gets one food item based on Id
        Task<FoodItem?> GetFoodItemByIdAsync(int id);

        // This method adds a new food item into database
        Task<FoodItem> AddFoodItemAsync(FoodItem foodItem);

        // This method updates an existing food item
        Task<FoodItem?> UpdateFoodItemAsync(int id, FoodItem foodItem);

        // This method deletes an existing food item based on Id
        Task<bool> DeleteFoodItemAsync(int id);
    }
}
