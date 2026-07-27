namespace FoodOrderAPI.DTOs
{
    // Contains the data required to create a food item.
    public class CreateFoodItemDto
    {
        // Name of the food item.
        public string Name { get; set; } = string.Empty;

        // Description of the food item.
        public string Description { get; set; } = string.Empty;

        // Price of the food item.
        public decimal Price { get; set; }

        // ID of the category to which the item belongs.
        public int FoodCategoryId { get; set; }

        // Indicates whether customers can order the item.
        public bool IsAvailable { get; set; }
    }
}