namespace FoodOrderAPI.Models
{
    // FoodItem represents one food/menu item in the restaurant
    // Example: Margherita Pizza, Chicken Burger, Coke, etc.
    public class FoodItem
    {
        // Primary key for FoodItems table
        public int Id { get; set; }

        // Name of the food item
        // Example: Margherita Pizza
        public string Name { get; set; } = string.Empty;

        // Description of the food item
        // Example: Cheese pizza with tomato sauce
        public string Description { get; set; } = string.Empty;

        // Price of the food item
        // Example: 250.00
        public decimal Price { get; set; }

        // Foreign key for FoodCategory table
        // This connects FoodItem with FoodCategory
        // Example: Pizza category Id = 1
        public int FoodCategoryId { get; set; }

        // Navigation property for FoodCategory
        // This helps EF Core load category details of this food item
        public FoodCategory FoodCategory { get; set; } = null!;

        // Tells whether this food item is currently available
        public bool IsAvailable { get; set; }
    }
}