using FoodOrderAPI.Data;
using FoodOrderAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace FoodOrderAPI.Repositories
{
    public class FoodCategoryRepository : IFoodCategoryRepository
    {
        // This field stores the database context object.
        // Repository uses ApplicationDbContext to communicate with the database.
        private readonly ApplicationDbContext _context;

        // Constructor injection.
        // ASP.NET Core automatically gives ApplicationDbContext object here.
        public FoodCategoryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // This method gets all food categories from the database.
        public async Task<IEnumerable<FoodCategory>> GetAllFoodCategoriesAsync()
        {
            return await _context.FoodCategories.ToListAsync();
        }

        // This method gets one food category based on id.
        // If category is not found, it returns null.
        public async Task<FoodCategory?> GetFoodCategoryByIdAsync(int id)
        {
            return await _context.FoodCategories
                .FirstOrDefaultAsync(category => category.Id == id);
        }

        // This method creates a new food category.
        public async Task<FoodCategory> CreateFoodCategoryAsync(FoodCategory foodCategory)
        {
            _context.FoodCategories.Add(foodCategory);

            await _context.SaveChangesAsync();

            return foodCategory;
        }

        // This method updates an existing food category.
        public async Task<FoodCategory?> UpdateFoodCategoryAsync(int id, FoodCategory foodCategory)
        {
            FoodCategory? existingCategory = await _context.FoodCategories
                .FirstOrDefaultAsync(category => category.Id == id);

            if (existingCategory == null)
            {
                return null;
            }

            // Update category name.
            existingCategory.CategoryName = foodCategory.CategoryName;

            await _context.SaveChangesAsync();

            return existingCategory;
        }

        // This method deletes a food category by id.
        public async Task<bool> DeleteFoodCategoryAsync(int id)
        {
            FoodCategory? existingCategory = await _context.FoodCategories
                .FirstOrDefaultAsync(category => category.Id == id);

            if (existingCategory == null)
            {
                return false;
            }

            _context.FoodCategories.Remove(existingCategory);

            await _context.SaveChangesAsync();

            return true;
        }

        // This method checks if the same food category name already exists.
        // ToLower() is used so "Pizza", "pizza", and "PIZZA" are treated as same.
        public async Task<bool> FoodCategoryExistsAsync(string categoryName)
        {
            return await _context.FoodCategories
                .AnyAsync(category =>
                    category.CategoryName.ToLower() == categoryName.ToLower());
        }
    }
}