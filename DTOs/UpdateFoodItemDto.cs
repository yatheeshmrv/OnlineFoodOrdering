namespace FoodOrderAPI.DTOs
{
    // Contains the data that can be changed
    // when updating a food item.
    public class UpdateFoodItemDto
    {
        // Updated name of the food item.
        public string Name { get; set; } = string.Empty;

        // Updated description of the food item.
        public string Description { get; set; } = string.Empty;

        // Updated price of the food item.
        public decimal Price { get; set; }

        // Updated image URL or relative image path.
        public string ImageUrl { get; set; } = string.Empty;

        // ID of the category to which the item belongs.
        public int FoodCategoryId { get; set; }

        // Indicates whether customers can order the item.
        public bool IsAvailable { get; set; }
    }
}