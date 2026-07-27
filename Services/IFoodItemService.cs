using FoodOrderAPI.DTOs;

namespace FoodOrderAPI.Services
{
    public interface IFoodItemService
    {
        // Gets all food items.
        Task<List<FoodItemDto>> GetAllFoodItemsAsync();

        // Gets food items using search, category filtering
        // and pagination.
        Task<PagedResponseDto<FoodItemDto>>
            GetPagedFoodItemsAsync(
                FoodItemQueryParametersDto queryParameters);

        // Gets one food item by its ID.
        Task<FoodItemDto?> GetFoodItemByIdAsync(int id);

        // Adds a new food item.
        Task<FoodItemDto> AddFoodItemAsync(
            FoodItemDto foodItemDto);

        // Updates an existing food item.
        Task<FoodItemDto?> UpdateFoodItemAsync(
            int id,
            FoodItemDto foodItemDto);

        // Deletes an existing food item.
        Task<bool> DeleteFoodItemAsync(int id);
    }
}