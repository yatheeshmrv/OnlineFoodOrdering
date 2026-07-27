namespace FoodOrderAPI.DTOs
{
    // Contains the data that can be changed
    // when updating a food category.
    public class UpdateFoodCategoryDto
    {
        // Updated name of the food category.
        public string CategoryName { get; set; } = string.Empty;

        // Indicates whether the category should be active.
        public bool IsActive { get; set; }
    }
}