using FoodOrderAPI.DTOs;

namespace FoodOrderAPI.Services
{
    public interface IFoodItemService
    {
        // Gets all food items
        Task<List<FoodItemDto>> GetAllFoodItemsAsync();

        // Gets one food item by Id
        Task<FoodItemDto?> GetFoodItemByIdAsync(int id);

        // Adds a new food item
        Task<FoodItemDto> AddFoodItemAsync(FoodItemDto foodItemDto);

        // Updates an existing food item
        Task<FoodItemDto?> UpdateFoodItemAsync(int id, FoodItemDto foodItemDto);

        // Deletes an existing food item
        Task<bool> DeleteFoodItemAsync(int id);
    }
}