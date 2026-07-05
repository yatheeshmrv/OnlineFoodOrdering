using FoodOrderAPI.Models;

namespace FoodOrderAPI.Repositories
{
    public interface IFoodItemRepository
    {
        Task<IEnumerable<FoodItem>> GetAllFoodItemsAsync();

        Task<FoodItem?> GetFoodItemByIdAsync(int id);

        Task<FoodItem> CreateFoodItemAsync(FoodItem foodItem);

        Task<FoodItem?> UpdateFoodItemAsync(int id, FoodItem foodItem);

        Task<bool> DeleteFoodItemAsync(int id);
    }
}
