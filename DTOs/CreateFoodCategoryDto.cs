namespace FoodOrderAPI.DTOs
{
    // Contains the data required to create a food category.
    public class CreateFoodCategoryDto
    {
        // Name of the new food category.
        public string CategoryName { get; set; } = string.Empty;

        // Indicates whether the category is active.
        public bool IsActive { get; set; }
    }
}