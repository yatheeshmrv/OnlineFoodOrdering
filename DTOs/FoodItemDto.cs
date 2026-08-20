namespace FoodOrderAPI.DTOs
{
    public class FoodItemDto
    {
        public int Id { get; set; }

        // Food item name.
        public string Name { get; set; } = string.Empty;

        // Food item description.
        public string Description { get; set; } = string.Empty;

        // Food item price.
        public decimal Price { get; set; }

        // Image URL/path displayed by the frontend.
        public string ImageUrl { get; set; } = string.Empty;

        // Foreign key category id.
        public int FoodCategoryId { get; set; }

        // This is used to display category name in GET response.
        public string? FoodCategoryName { get; set; }

        // Shows whether food item is available.
        public bool IsAvailable { get; set; }
    }
}