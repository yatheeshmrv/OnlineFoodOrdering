using FoodOrderAPI.Data;
using FoodOrderAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace FoodOrderAPI.Repositories
{
    public class FoodItemRepository : IFoodItemRepository
    {
        // Used to communicate with the SQL Server database.
        private readonly ApplicationDbContext _context;

        // Receives ApplicationDbContext through dependency injection.
        public FoodItemRepository(ApplicationDbContext context)
        {
            // Stores the injected database context.
            _context = context;
        }

        // Gets all food items together with their categories.
        public async Task<List<FoodItem>>
            GetAllFoodItemsAsync()
        {
            return await _context.FoodItems
                .Include(foodItem => foodItem.FoodCategory)
                .AsNoTracking()
                .ToListAsync();
        }

        // Gets a single page of food items after applying
        // optional search and category filters.
        public async Task<(List<FoodItem> Items, int TotalCount)>
            GetPagedFoodItemsAsync(
                string? search,
                int? categoryId,
                int pageNumber,
                int pageSize)
        {
            // Begins with all food items.
            // IQueryable builds the SQL query gradually.
            var query = _context.FoodItems
                .Include(foodItem => foodItem.FoodCategory)
                .AsNoTracking()
                .AsQueryable();

            // Applies the search filter only when text is provided.
            if (!string.IsNullOrWhiteSpace(search))
            {
                // Removes unnecessary spaces from the search text.
                var searchTerm = search.Trim();

                // Searches both the item name and description.
                query = query.Where(foodItem =>
                    foodItem.Name.Contains(searchTerm) ||
                    foodItem.Description.Contains(searchTerm));
            }

            // Applies the category filter when a category ID is given.
            if (categoryId.HasValue)
            {
                query = query.Where(foodItem =>
                    foodItem.FoodCategoryId ==
                    categoryId.Value);
            }

            // Counts all matching records before pagination.
            // This is required to calculate the total number of pages.
            var totalCount = await query.CountAsync();

            // Calculates how many records must be skipped.
            var recordsToSkip =
                (pageNumber - 1) * pageSize;

            // Orders the results to keep pagination consistent,
            // skips previous pages and retrieves the requested page.
            var items = await query
                .OrderBy(foodItem => foodItem.Id)
                .Skip(recordsToSkip)
                .Take(pageSize)
                .ToListAsync();

            // Returns both the current page and total matching count.
            return (items, totalCount);
        }

        // Gets one food item based on its ID.
        public async Task<FoodItem?>
            GetFoodItemByIdAsync(int id)
        {
            return await _context.FoodItems
                .Include(foodItem => foodItem.FoodCategory)
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    foodItem => foodItem.Id == id);
        }

        // Adds a new food item to the database.
        public async Task<FoodItem>
            AddFoodItemAsync(FoodItem foodItem)
        {
            // Adds the new food item to EF Core tracking.
            await _context.FoodItems.AddAsync(foodItem);

            // Saves the new food item to the database.
            await _context.SaveChangesAsync();

            // Reloads and returns the item with its category.
            return await _context.FoodItems
                .Include(item => item.FoodCategory)
                .AsNoTracking()
                .FirstAsync(item => item.Id == foodItem.Id);
        }

        // Updates an existing food item.
        public async Task<FoodItem?>
            UpdateFoodItemAsync(
                int id,
                FoodItem foodItem)
        {
            // Finds the existing food item using the route ID.
            var existingFoodItem =
                await _context.FoodItems.FindAsync(id);

            // Returns null when the food item does not exist.
            if (existingFoodItem == null)
            {
                return null;
            }

            // Copies the new values to the existing entity.
            existingFoodItem.Name = foodItem.Name;
            existingFoodItem.Description =
                foodItem.Description;
            existingFoodItem.Price = foodItem.Price;
            existingFoodItem.ImageUrl = foodItem.ImageUrl;
            existingFoodItem.FoodCategoryId =
                foodItem.FoodCategoryId;
            existingFoodItem.IsAvailable =
                foodItem.IsAvailable;

            // Saves the changes to the database.
            await _context.SaveChangesAsync();

            // Returns the updated food item.
            return existingFoodItem;
        }

        // Deletes an existing food item based on its ID.
        public async Task<bool>
            DeleteFoodItemAsync(int id)
        {
            // Finds the existing food item.
            var existingFoodItem =
                await _context.FoodItems
                    .FirstOrDefaultAsync(
                        item => item.Id == id);

            // Returns false when the item does not exist.
            if (existingFoodItem == null)
            {
                return false;
            }

            // Marks the food item for deletion.
            _context.FoodItems.Remove(existingFoodItem);

            // Sends the DELETE command to SQL Server.
            await _context.SaveChangesAsync();

            // Confirms that deletion succeeded.
            return true;
        }
    }
}