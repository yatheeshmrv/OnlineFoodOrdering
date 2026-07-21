using FoodOrderAPI.DTOs;
using FoodOrderAPI.Models;
using FoodOrderAPI.Repositories;

namespace FoodOrderAPI.Services
{
    public class FoodItemService : IFoodItemService
    {
        // Repository used for FoodItem database operations.
        private readonly IFoodItemRepository _foodItemRepository;

        // Repository used to verify whether a FoodCategory exists.
        private readonly IFoodCategoryRepository _foodCategoryRepository;

        // Constructor injection.
        // ASP.NET Core provides the repository objects automatically.
        public FoodItemService(
            IFoodItemRepository foodItemRepository,
            IFoodCategoryRepository foodCategoryRepository)
        {
            _foodItemRepository = foodItemRepository;
            _foodCategoryRepository = foodCategoryRepository;
        }

        // Validates the FoodItemDto before creating or updating a food item.
        private async Task ValidateFoodItemAsync(FoodItemDto foodItemDto)
        {
            // Food item name must not be empty.
            if (string.IsNullOrWhiteSpace(foodItemDto.Name))
            {
                throw new ArgumentException(
                    "Food item name is required.");
            }

            // Price must be greater than zero.
            if (foodItemDto.Price <= 0)
            {
                throw new ArgumentException(
                    "Food item price must be greater than 0.");
            }

            // FoodCategoryId must contain a valid positive value.
            if (foodItemDto.FoodCategoryId <= 0)
            {
                throw new ArgumentException(
                    "Valid food category is required.");
            }

            // Check whether the category exists in the database.
            // This prevents a foreign-key error.
            var foodCategory =
                await _foodCategoryRepository
                    .GetFoodCategoryByIdAsync(
                        foodItemDto.FoodCategoryId);

            if (foodCategory == null)
            {
                throw new ArgumentException(
                    "Food category does not exist.");
            }
        }

        // Converts a FoodItem entity into a FoodItemDto.
        // Keeping the mapping in one method avoids repeating the same code.
        private static FoodItemDto MapFoodItemToDto(
            FoodItem foodItem)
        {
            return new FoodItemDto
            {
                Id = foodItem.Id,
                Name = foodItem.Name,
                Description = foodItem.Description,
                Price = foodItem.Price,
                FoodCategoryId = foodItem.FoodCategoryId,

                // FoodCategory must be loaded by the repository
                // for the category name to appear.
                FoodCategoryName =
                    foodItem.FoodCategory?.CategoryName,

                IsAvailable = foodItem.IsAvailable
            };
        }

        // Gets all food items.
        public async Task<List<FoodItemDto>>
            GetAllFoodItemsAsync()
        {
            var foodItems =
                await _foodItemRepository
                    .GetAllFoodItemsAsync();

            return foodItems
                .Select(MapFoodItemToDto)
                .ToList();
        }

        // Gets one food item by its ID.
        public async Task<FoodItemDto?>
            GetFoodItemByIdAsync(int id)
        {
            var foodItem =
                await _foodItemRepository
                    .GetFoodItemByIdAsync(id);

            if (foodItem == null)
            {
                return null;
            }

            return MapFoodItemToDto(foodItem);
        }

        // Creates a new food item.
        public async Task<FoodItemDto> AddFoodItemAsync(
            FoodItemDto foodItemDto)
        {
            // Validate the request before saving.
            await ValidateFoodItemAsync(foodItemDto);

            // Convert the DTO into a FoodItem entity.
            var foodItem = new FoodItem
            {
                Name = foodItemDto.Name.Trim(),
                Description = foodItemDto.Description?.Trim(),
                Price = foodItemDto.Price,
                FoodCategoryId = foodItemDto.FoodCategoryId,
                IsAvailable = foodItemDto.IsAvailable
            };

            // Save the food item.
            var addedFoodItem =
                await _foodItemRepository
                    .AddFoodItemAsync(foodItem);

            // Retrieve the saved food item again.
            // GetFoodItemByIdAsync uses Include() and loads FoodCategory.
            var savedFoodItem =
                await _foodItemRepository
                    .GetFoodItemByIdAsync(addedFoodItem.Id);

            if (savedFoodItem == null)
            {
                throw new InvalidOperationException(
                    "The food item was created but could not be retrieved.");
            }

            // FoodCategoryName will now be available.
            return MapFoodItemToDto(savedFoodItem);
        }

        // Updates an existing food item.
        public async Task<FoodItemDto?> UpdateFoodItemAsync(
            int id,
            FoodItemDto foodItemDto)
        {
            // Validate the request before updating.
            await ValidateFoodItemAsync(foodItemDto);

            // Convert the DTO into a FoodItem entity.
            var foodItem = new FoodItem
            {
                Name = foodItemDto.Name.Trim(),
                Description = foodItemDto.Description?.Trim(),
                Price = foodItemDto.Price,
                FoodCategoryId = foodItemDto.FoodCategoryId,
                IsAvailable = foodItemDto.IsAvailable
            };

            // Update the existing item.
            var updatedFoodItem =
                await _foodItemRepository
                    .UpdateFoodItemAsync(id, foodItem);

            if (updatedFoodItem == null)
            {
                return null;
            }

            // Retrieve it again with the related FoodCategory.
            var savedFoodItem =
                await _foodItemRepository
                    .GetFoodItemByIdAsync(updatedFoodItem.Id);

            if (savedFoodItem == null)
            {
                throw new InvalidOperationException(
                    "The food item was updated but could not be retrieved.");
            }

            // FoodCategoryName will now be included in the response.
            return MapFoodItemToDto(savedFoodItem);
        }

        // Deletes an existing food item.
        public async Task<bool> DeleteFoodItemAsync(int id)
        {
            return await _foodItemRepository
                .DeleteFoodItemAsync(id);
        }
    }
}