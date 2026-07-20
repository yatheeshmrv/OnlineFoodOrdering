using FoodOrderAPI.Models;
using FoodOrderAPI.Repositories;

namespace FoodOrderAPI.Services
{
    public class FoodCategoryService : IFoodCategoryService
    {
        // This field stores the repository object.
        // Service uses repository to perform database-related operations.
        private readonly IFoodCategoryRepository _foodCategoryRepository;

        // Constructor injection.
        // ASP.NET Core automatically provides IFoodCategoryRepository object here.
        public FoodCategoryService(IFoodCategoryRepository foodCategoryRepository)
        {
            _foodCategoryRepository = foodCategoryRepository;
        }

        // This method gets all food categories.
        // Repository returns IEnumerable<FoodCategory>.
        // Service converts it to List<FoodCategory>.
        public async Task<List<FoodCategory>> GetAllFoodCategoriesAsync()
        {
            IEnumerable<FoodCategory> foodCategories =
                await _foodCategoryRepository.GetAllFoodCategoriesAsync();

            List<FoodCategory> foodCategoryList = foodCategories.ToList();

            return foodCategoryList;
        }

        // This method gets one food category by id.
        // If category is not found, it returns null.
        public async Task<FoodCategory?> GetFoodCategoryByIdAsync(int id)
        {
            FoodCategory? foodCategory =
                await _foodCategoryRepository.GetFoodCategoryByIdAsync(id);

            return foodCategory;
        }

        public async Task<FoodCategory> CreateFoodCategoryAsync(FoodCategory foodCategory)
        {
            // Checks whether category name is empty.
            if (string.IsNullOrWhiteSpace(foodCategory.CategoryName))
            {
                throw new ArgumentException("Category name is required.");
            }

            // Trim removes unwanted spaces.
            // Example: " Pizza " becomes "Pizza".
            foodCategory.CategoryName = foodCategory.CategoryName.Trim();

            // Checks whether the same category already exists.
            bool categoryAlreadyExists =
                await _foodCategoryRepository.FoodCategoryExistsAsync(foodCategory.CategoryName);

            // If category already exists, stop creation.
            if (categoryAlreadyExists)
            {
                throw new ArgumentException("Food category already exists.");
            }

            // If validation passes, create the category.
            FoodCategory createdFoodCategory =
                await _foodCategoryRepository.CreateFoodCategoryAsync(foodCategory);

            return createdFoodCategory;
        }

        // This method updates an existing food category.
        public async Task<FoodCategory?> UpdateFoodCategoryAsync(int id, FoodCategory foodCategory)
        {
            // Basic validation.
            // Category name should not be empty while updating.
            if (string.IsNullOrWhiteSpace(foodCategory.CategoryName))
            {
                throw new ArgumentException("Category name is required.");
            }

            FoodCategory? updatedFoodCategory =
                await _foodCategoryRepository.UpdateFoodCategoryAsync(id, foodCategory);

            return updatedFoodCategory;
        }

        // This method deletes a food category by id.
        // Returns true if deleted.
        // Returns false if category is not found.
        public async Task<bool> DeleteFoodCategoryAsync(int id)
        {
            bool isDeleted =
                await _foodCategoryRepository.DeleteFoodCategoryAsync(id);

            return isDeleted;
        }
    }
}