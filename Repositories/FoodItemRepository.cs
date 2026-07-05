using FoodOrderAPI.Models;

namespace FoodOrderAPI.Repositories
{
    public class FoodItemRepository : IFoodItemRepository
    {
        private static readonly List<FoodItem> foodItems = new List<FoodItem>
        {
            new FoodItem
            {
                Id = 1,
                Name = "Chicken Biryani",
                Description = "Spicy chicken biryani",
                Price = 180,
                Category = "Main Course",
                IsAvailable = true
            },
            new FoodItem
            {
                Id = 2,
                Name = "Veg Fried Rice",
                Description = "Fried rice with vegetables",
                Price = 120,
                Category = "Main Course",
                IsAvailable = true
            }
        };

        public async Task<IEnumerable<FoodItem>> GetAllFoodItemsAsync()
        {
            return await Task.FromResult(foodItems);
        }

        public async Task<FoodItem?> GetFoodItemByIdAsync(int id)
        {
            var foodItem = foodItems.FirstOrDefault(x => x.Id == id);

            return await Task.FromResult(foodItem);
        }

        public async Task<FoodItem> CreateFoodItemAsync(FoodItem foodItem)
        {
            foodItem.Id = foodItems.Count == 0 ? 1 : foodItems.Max(x => x.Id) + 1;

            foodItems.Add(foodItem);

            return await Task.FromResult(foodItem);
        }

        public async Task<FoodItem?> UpdateFoodItemAsync(int id, FoodItem foodItem)
        {
            var existingFoodItem = foodItems.FirstOrDefault(x => x.Id == id);

            if (existingFoodItem == null)
            {
                return null;
            }

            existingFoodItem.Name = foodItem.Name;
            existingFoodItem.Description = foodItem.Description;
            existingFoodItem.Price = foodItem.Price;
            existingFoodItem.Category = foodItem.Category;
            existingFoodItem.IsAvailable = foodItem.IsAvailable;

            return await Task.FromResult(existingFoodItem);
        }

        public async Task<bool> DeleteFoodItemAsync(int id)
        {
            var existingFoodItem = foodItems.FirstOrDefault(x => x.Id == id);

            if (existingFoodItem == null)
            {
                return await Task.FromResult(false);
            }

            foodItems.Remove(existingFoodItem);

            return await Task.FromResult(true);
        }
    }
}
