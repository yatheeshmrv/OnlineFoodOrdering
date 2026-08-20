using FoodOrderAPI.DTOs;
using FoodOrderAPI.Models;
using FoodOrderAPI.Repositories;

namespace FoodOrderAPI.Services
{
    public class FoodItemService : IFoodItemService
    {
        // Repository used for food-item database operations.
        private readonly IFoodItemRepository
            _foodItemRepository;

        // Repository used to verify whether a category exists.
        private readonly IFoodCategoryRepository
            _foodCategoryRepository;

        // Receives the repositories through dependency injection.
        public FoodItemService(
            IFoodItemRepository foodItemRepository,
            IFoodCategoryRepository foodCategoryRepository)
        {
            // Stores the food-item repository.
            _foodItemRepository = foodItemRepository;

            // Stores the food-category repository.
            _foodCategoryRepository =
                foodCategoryRepository;
        }

        // Validates a FoodItemDto before creating
        // or updating a food item.
        private async Task ValidateFoodItemAsync(
            FoodItemDto foodItemDto)
        {
            // Ensures that the food-item name is provided.
            if (string.IsNullOrWhiteSpace(
                foodItemDto.Name))
            {
                throw new ArgumentException(
                    "Food item name is required.");
            }

            // Ensures that the price is greater than zero.
            if (foodItemDto.Price <= 0)
            {
                throw new ArgumentException(
                    "Food item price must be greater than 0.");
            }

            // Ensures that a positive category ID is provided.
            if (foodItemDto.FoodCategoryId <= 0)
            {
                throw new ArgumentException(
                    "Valid food category is required.");
            }

            // Checks whether the selected category exists.
            var foodCategory =
                await _foodCategoryRepository
                    .GetFoodCategoryByIdAsync(
                        foodItemDto.FoodCategoryId);

            // Prevents a database foreign-key error.
            if (foodCategory == null)
            {
                throw new ArgumentException(
                    "Food category does not exist.");
            }
        }

        // Converts a FoodItem entity into a FoodItemDto.
        private static FoodItemDto MapFoodItemToDto(
            FoodItem foodItem)
        {
            return new FoodItemDto
            {
                Id = foodItem.Id,
                Name = foodItem.Name,
                Description = foodItem.Description,
                Price = foodItem.Price,
                ImageUrl = foodItem.ImageUrl,
                FoodCategoryId =
                foodItem.FoodCategoryId,

                // The category must be loaded with Include()
                // for its name to be available.
                FoodCategoryName =
                    foodItem.FoodCategory?.CategoryName,

                IsAvailable = foodItem.IsAvailable
            };
        }

        // Gets every food item without pagination.
        public async Task<List<FoodItemDto>>
            GetAllFoodItemsAsync()
        {
            // Gets all food-item entities.
            var foodItems =
                await _foodItemRepository
                    .GetAllFoodItemsAsync();

            // Maps the entities to DTOs.
            return foodItems
                .Select(MapFoodItemToDto)
                .ToList();
        }

        // Gets a filtered and paginated collection
        // of food items.
        public async Task<PagedResponseDto<FoodItemDto>>
            GetPagedFoodItemsAsync(
                FoodItemQueryParametersDto queryParameters)
        {
            // Sends the search, category and pagination
            // values to the repository.
            var result =
                await _foodItemRepository
                    .GetPagedFoodItemsAsync(
                        queryParameters.Search,
                        queryParameters.CategoryId,
                        queryParameters.PageNumber,
                        queryParameters.PageSize);

            // Converts the retrieved entities into DTOs.
            var foodItemDtos = result.Items
                .Select(MapFoodItemToDto)
                .ToList();

            // Calculates the number of available pages.
            var totalPages = (int)Math.Ceiling(
                result.TotalCount /
                (double)queryParameters.PageSize);

            // Creates the complete paginated response.
            return new PagedResponseDto<FoodItemDto>
            {
                Items = foodItemDtos,
                PageNumber =
                    queryParameters.PageNumber,
                PageSize =
                    queryParameters.PageSize,
                TotalCount =
                    result.TotalCount,
                TotalPages =
                    totalPages
            };
        }

        // Gets one food item by its ID.
        public async Task<FoodItemDto?>
            GetFoodItemByIdAsync(int id)
        {
            // Gets the food-item entity.
            var foodItem =
                await _foodItemRepository
                    .GetFoodItemByIdAsync(id);

            // Returns null when the item does not exist.
            if (foodItem == null)
            {
                return null;
            }

            // Maps the entity to a DTO.
            return MapFoodItemToDto(foodItem);
        }

        // Creates a new food item.
        public async Task<FoodItemDto>
            AddFoodItemAsync(FoodItemDto foodItemDto)
        {
            // Validates the request before saving.
            await ValidateFoodItemAsync(foodItemDto);

            // Converts the DTO into an entity.
            var foodItem = new FoodItem
            {
                Name = foodItemDto.Name.Trim(),
                Description =
                    foodItemDto.Description?.Trim(),
                Price = foodItemDto.Price,
                ImageUrl =
    (foodItemDto.ImageUrl ?? string.Empty).Trim(),
                FoodCategoryId =
    foodItemDto.FoodCategoryId,
                IsAvailable =
                    foodItemDto.IsAvailable
            };

            // Saves the food item.
            var addedFoodItem =
                await _foodItemRepository
                    .AddFoodItemAsync(foodItem);

            // Retrieves the saved item with its category.
            var savedFoodItem =
                await _foodItemRepository
                    .GetFoodItemByIdAsync(
                        addedFoodItem.Id);

            // Handles an unexpected retrieval failure.
            if (savedFoodItem == null)
            {
                throw new InvalidOperationException(
                    "The food item was created but " +
                    "could not be retrieved.");
            }

            // Returns the mapped food item.
            return MapFoodItemToDto(savedFoodItem);
        }

        // Updates an existing food item.
        public async Task<FoodItemDto?>
            UpdateFoodItemAsync(
                int id,
                FoodItemDto foodItemDto)
        {
            // Validates the request before updating.
            await ValidateFoodItemAsync(foodItemDto);

            // Converts the DTO into an entity.
            var foodItem = new FoodItem
            {
                Name = foodItemDto.Name.Trim(),
                Description =
                    foodItemDto.Description?.Trim(),
                Price = foodItemDto.Price,
                ImageUrl =
    (foodItemDto.ImageUrl ?? string.Empty).Trim(),
                FoodCategoryId =
    foodItemDto.FoodCategoryId,
                IsAvailable =
                    foodItemDto.IsAvailable
            };

            // Updates the existing item.
            var updatedFoodItem =
                await _foodItemRepository
                    .UpdateFoodItemAsync(
                        id,
                        foodItem);

            // Returns null when the item does not exist.
            if (updatedFoodItem == null)
            {
                return null;
            }

            // Retrieves the updated item with its category.
            var savedFoodItem =
                await _foodItemRepository
                    .GetFoodItemByIdAsync(
                        updatedFoodItem.Id);

            // Handles an unexpected retrieval failure.
            if (savedFoodItem == null)
            {
                throw new InvalidOperationException(
                    "The food item was updated but " +
                    "could not be retrieved.");
            }

            // Returns the mapped food item.
            return MapFoodItemToDto(savedFoodItem);
        }

        // Deletes an existing food item.
        public async Task<bool>
            DeleteFoodItemAsync(int id)
        {
            return await _foodItemRepository
                .DeleteFoodItemAsync(id);
        }
    }
}