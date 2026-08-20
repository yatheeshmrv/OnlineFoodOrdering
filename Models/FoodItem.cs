namespace FoodOrderAPI.Models
{
    // FoodItem represents one food/menu item in the restaurant.
    // Example: Margherita Pizza, Chicken Burger, Coke, etc.
    public class FoodItem
    {
        // Primary key for FoodItems table.
        public int Id { get; set; }

        // Name of the food item.
        public string Name { get; set; } = string.Empty;

        // Description of the food item.
        public string Description { get; set; } = string.Empty;

        // Price of the food item.
        public decimal Price { get; set; }

        // Stores the image URL/path used by the frontend.
        public string ImageUrl { get; set; } = string.Empty;

        // Foreign key for FoodCategory table.
        public int FoodCategoryId { get; set; }

        // Navigation property for FoodCategory.
        public FoodCategory FoodCategory { get; set; } = null!;

        // Tells whether this food item is currently available.
        public bool IsAvailable { get; set; }
    }
}