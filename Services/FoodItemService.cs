using FoodOrderAPI.DTOs;
using FoodOrderAPI.Models;
using FoodOrderAPI.Repositories;

namespace FoodOrderAPI.Services
{
    public class FoodItemService : IFoodItemService
    {
        private readonly IFoodItemRepository _foodItemRepository;

        public FoodItemService(IFoodItemRepository foodItemRepository)
        {
            _foodItemRepository = foodItemRepository;
        }

        public async Task<IEnumerable<FoodItemDto>> GetAllFoodItemsAsync()
        {
            var foodItems = await _foodItemRepository.GetAllFoodItemsAsync();

            return foodItems.Select(foodItem => MapToDto(foodItem));
        }

        public async Task<FoodItemDto?> GetFoodItemByIdAsync(int id)
        {
            var foodItem = await _foodItemRepository.GetFoodItemByIdAsync(id);

            if (foodItem == null)
            {
                return null;
            }

            return MapToDto(foodItem);
        }

        public async Task<FoodItemDto> CreateFoodItemAsync(CreateFoodItemDto createFoodItemDto)
        {
            var foodItem = new FoodItem
            {
                Name = createFoodItemDto.Name,
                Description = createFoodItemDto.Description,
                Price = createFoodItemDto.Price,
                Category = createFoodItemDto.Category,
                IsAvailable = createFoodItemDto.IsAvailable
            };

            var createdFoodItem = await _foodItemRepository.CreateFoodItemAsync(foodItem);

            return MapToDto(createdFoodItem);
        }

        public async Task<FoodItemDto?> UpdateFoodItemAsync(int id, UpdateFoodItemDto updateFoodItemDto)
        {
            var foodItem = new FoodItem
            {
                Name = updateFoodItemDto.Name,
                Description = updateFoodItemDto.Description,
                Price = updateFoodItemDto.Price,
                Category = updateFoodItemDto.Category,
                IsAvailable = updateFoodItemDto.IsAvailable
            };

            var updatedFoodItem = await _foodItemRepository.UpdateFoodItemAsync(id, foodItem);

            if (updatedFoodItem == null)
            {
                return null;
            }

            return MapToDto(updatedFoodItem);
        }

        public async Task<bool> DeleteFoodItemAsync(int id)
        {
            return await _foodItemRepository.DeleteFoodItemAsync(id);
        }

        private FoodItemDto MapToDto(FoodItem foodItem)
        {
            return new FoodItemDto
            {
                Id = foodItem.Id,
                Name = foodItem.Name,
                Description = foodItem.Description,
                Price = foodItem.Price,
                Category = foodItem.Category,
                IsAvailable = foodItem.IsAvailable
            };
        }
    }
}
