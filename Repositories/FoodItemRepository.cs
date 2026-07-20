using FoodOrderAPI.Data;
using FoodOrderAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace FoodOrderAPI.Repositories
{
    public class FoodItemRepository : IFoodItemRepository
    {
        // ApplicationDbContext is used to communicate with SQL Server database
        private readonly ApplicationDbContext _context;

        // Constructor Injection
        // ASP.NET Core provides ApplicationDbContext object automatically
        public FoodItemRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // This method gets all food items from database
        // Include is used to load related FoodCategory data also
        public async Task<List<FoodItem>> GetAllFoodItemsAsync()
        {
            return await _context.FoodItems
                .Include(foodItem => foodItem.FoodCategory)
                .AsNoTracking()
                .ToListAsync();
        }

        // This method gets one food item based on Id
        // Include loads category details with the food item
        public async Task<FoodItem?> GetFoodItemByIdAsync(int id)
        {
            return await _context.FoodItems
                .Include(foodItem => foodItem.FoodCategory)
                .AsNoTracking()
                .FirstOrDefaultAsync(foodItem => foodItem.Id == id);
        }

        // This method adds a new food item into the database
        public async Task<FoodItem> AddFoodItemAsync(FoodItem foodItem)
        {
            // AddAsync prepares INSERT operation
            await _context.FoodItems.AddAsync(foodItem);

            // SaveChangesAsync actually saves the new row into SQL Server
            await _context.SaveChangesAsync();

            // Return newly added food item with generated Id
            return foodItem;
        }

        public async Task<FoodItem?> UpdateFoodItemAsync(int id, FoodItem foodItem)
        {
            // First, find the existing food item from database using route id
            var existingFoodItem = await _context.FoodItems.FindAsync(id);

            // If food item is not found, return null
            if (existingFoodItem == null)
            {
                return null;
            }

            // Update existing food item values with new values from request body
            existingFoodItem.Name = foodItem.Name;
            existingFoodItem.Description = foodItem.Description;
            existingFoodItem.Price = foodItem.Price;
            existingFoodItem.FoodCategoryId = foodItem.FoodCategoryId;
            existingFoodItem.IsAvailable = foodItem.IsAvailable;

            // Save updated values into database
            await _context.SaveChangesAsync();

            // Return updated food item
            return existingFoodItem;
        }

        // This method deletes an existing food item based on Id
        public async Task<bool> DeleteFoodItemAsync(int id)
        {
            // First, find the existing food item from database
            var existingFoodItem = await _context.FoodItems
                .FirstOrDefaultAsync(item => item.Id == id);

            // If food item is not found, return false
            if (existingFoodItem == null)
            {
                return false;
            }

            // Remove prepares DELETE operation
            _context.FoodItems.Remove(existingFoodItem);

            // SaveChangesAsync sends DELETE command to SQL Server
            await _context.SaveChangesAsync();

            // Return true because delete was successful
            return true;
        }
    }
}