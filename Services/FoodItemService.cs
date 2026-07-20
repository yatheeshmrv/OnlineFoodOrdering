using FoodOrderAPI.DTOs;
using FoodOrderAPI.Models;
using FoodOrderAPI.Repositories;

namespace FoodOrderAPI.Services
{
    public class FoodItemService : IFoodItemService
    {
        // Repository object is used to call FoodItem database-related methods
        private readonly IFoodItemRepository _foodItemRepository;

        // Category repository is used to check whether FoodCategoryId really exists in database
        private readonly IFoodCategoryRepository _foodCategoryRepository;

        // Constructor Injection
        // ASP.NET Core provides FoodItemRepository and FoodCategoryRepository objects automatically
        public FoodItemService(
            IFoodItemRepository foodItemRepository,
            IFoodCategoryRepository foodCategoryRepository)
        {
            _foodItemRepository = foodItemRepository;
            _foodCategoryRepository = foodCategoryRepository;
        }

        // This private method keeps FoodItem validation in one place
        // So Add and Update do not repeat the same validation code again and again
        private async Task ValidateFoodItemAsync(FoodItemDto foodItemDto)
        {
            // Checks whether food item name is empty
            if (string.IsNullOrWhiteSpace(foodItemDto.Name))
            {
                throw new ArgumentException("Food item name is required.");
            }

            // Checks whether price is valid
            if (foodItemDto.Price <= 0)
            {
                throw new ArgumentException("Food item price must be greater than 0.");
            }

            // Checks whether FoodCategoryId is greater than 0
            if (foodItemDto.FoodCategoryId <= 0)
            {
                throw new ArgumentException("Valid food category is required.");
            }

            // Checks whether the given FoodCategoryId actually exists in the database
            // This prevents foreign key error and avoids 500 Internal Server Error
            var foodCategory = await _foodCategoryRepository.GetFoodCategoryByIdAsync(foodItemDto.FoodCategoryId);

            if (foodCategory == null)
            {
                throw new ArgumentException("Food category does not exist.");
            }
        }

        // This method gets all food items
        // It converts FoodItem Entity list into FoodItemDto list
        public async Task<List<FoodItemDto>> GetAllFoodItemsAsync()
        {
            var foodItems = await _foodItemRepository.GetAllFoodItemsAsync();

            return foodItems.Select(foodItem => new FoodItemDto
            {
                Id = foodItem.Id,
                Name = foodItem.Name,
                Description = foodItem.Description,
                Price = foodItem.Price,
                FoodCategoryId = foodItem.FoodCategoryId,

                // This will show category name in GET response
                FoodCategoryName = foodItem.FoodCategory != null
                    ? foodItem.FoodCategory.CategoryName
                    : null,

                IsAvailable = foodItem.IsAvailable
            }).ToList();
        }

        // This method gets one food item by Id
        // It converts FoodItem Entity into FoodItemDto
        public async Task<FoodItemDto?> GetFoodItemByIdAsync(int id)
        {
            var foodItem = await _foodItemRepository.GetFoodItemByIdAsync(id);

            if (foodItem == null)
            {
                return null;
            }

            return new FoodItemDto
            {
                Id = foodItem.Id,
                Name = foodItem.Name,
                Description = foodItem.Description,
                Price = foodItem.Price,
                FoodCategoryId = foodItem.FoodCategoryId,

                // This will show category name in GET by Id response
                FoodCategoryName = foodItem.FoodCategory != null
                    ? foodItem.FoodCategory.CategoryName
                    : null,

                IsAvailable = foodItem.IsAvailable
            };
        }

        // This method adds a new food item
        // It validates DTO, converts DTO to Entity, then sends it to Repository
        public async Task<FoodItemDto> AddFoodItemAsync(FoodItemDto foodItemDto)
        {
            // Validates name, price, FoodCategoryId, and category existence
            await ValidateFoodItemAsync(foodItemDto);

            // Convert DTO to Entity
            var foodItem = new FoodItem
            {
                Name = foodItemDto.Name,
                Description = foodItemDto.Description,
                Price = foodItemDto.Price,
                FoodCategoryId = foodItemDto.FoodCategoryId,
                IsAvailable = foodItemDto.IsAvailable
            };

            var addedFoodItem = await _foodItemRepository.AddFoodItemAsync(foodItem);

            // Convert added Entity back to DTO
            return new FoodItemDto
            {
                Id = addedFoodItem.Id,
                Name = addedFoodItem.Name,
                Description = addedFoodItem.Description,
                Price = addedFoodItem.Price,
                FoodCategoryId = addedFoodItem.FoodCategoryId,
                IsAvailable = addedFoodItem.IsAvailable
            };
        }

        // This method updates an existing food item
        // It validates DTO, converts DTO to Entity, then sends it to Repository
        public async Task<FoodItemDto?> UpdateFoodItemAsync(int id, FoodItemDto foodItemDto)
        {
            // Validates name, price, FoodCategoryId, and category existence
            await ValidateFoodItemAsync(foodItemDto);

            // Convert DTO to Entity
            var foodItem = new FoodItem
            {
                Name = foodItemDto.Name,
                Description = foodItemDto.Description,
                Price = foodItemDto.Price,
                FoodCategoryId = foodItemDto.FoodCategoryId,
                IsAvailable = foodItemDto.IsAvailable
            };

            var updatedFoodItem = await _foodItemRepository.UpdateFoodItemAsync(id, foodItem);

            if (updatedFoodItem == null)
            {
                return null;
            }

            // Convert updated Entity back to DTO
            return new FoodItemDto
            {
                Id = updatedFoodItem.Id,
                Name = updatedFoodItem.Name,
                Description = updatedFoodItem.Description,
                Price = updatedFoodItem.Price,
                FoodCategoryId = updatedFoodItem.FoodCategoryId,
                IsAvailable = updatedFoodItem.IsAvailable
            };
        }

        // This method deletes an existing food item
        public async Task<bool> DeleteFoodItemAsync(int id)
        {
            return await _foodItemRepository.DeleteFoodItemAsync(id);
        }
    }
}