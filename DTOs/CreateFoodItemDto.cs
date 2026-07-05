using System.ComponentModel.DataAnnotations;

namespace FoodOrderAPI.DTOs
{
    public class CreateFoodItemDto
    {
        [Required(ErrorMessage = "Food item name is required")]
        [StringLength(100, ErrorMessage = "Food item name cannot be more than 100 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required")]
        [StringLength(250, ErrorMessage = "Description cannot be more than 250 characters")]
        public string Description { get; set; } = string.Empty;

        [Range(1, 10000, ErrorMessage = "Price must be between 1 and 10000")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Category is required")]
        [StringLength(50, ErrorMessage = "Category cannot be more than 50 characters")]
        public string Category { get; set; } = string.Empty;

        public bool IsAvailable { get; set; }
    }
}