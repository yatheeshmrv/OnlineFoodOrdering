using System.ComponentModel.DataAnnotations;

namespace FoodOrderAPI.DTOs
{
    // This DTO is used when adding a new food category
    // It contains only the fields required from the API request body
    public class CreateFoodCategoryDto
    {
        // Required means CategoryName cannot be empty or missing
        // StringLength limits the maximum number of characters allowed
        [Required(ErrorMessage = "Category name is required.")]
        [StringLength(100, ErrorMessage = "Category name cannot exceed 100 characters.")]
        public string CategoryName { get; set; } = string.Empty;

        // IsActive tells whether this category is active or not
        public bool IsActive { get; set; }
    }
}
