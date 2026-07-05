using FoodOrderAPI.DTOs;

namespace FoodOrderAPI.Services
{
    public interface IFoodItemService
    {
        Task<IEnumerable<FoodItemDto>> GetAllFoodItemsAsync();

        Task<FoodItemDto?> GetFoodItemByIdAsync(int id);

        Task<FoodItemDto> CreateFoodItemAsync(CreateFoodItemDto createFoodItemDto);

        Task<FoodItemDto?> UpdateFoodItemAsync(int id, UpdateFoodItemDto updateFoodItemDto);

        Task<bool> DeleteFoodItemAsync(int id);
    }
}
