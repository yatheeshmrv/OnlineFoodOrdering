using System.ComponentModel.DataAnnotations;

namespace FoodOrderAPI.DTOs
{
    public class CreateOrderDto
    {
        [Required(ErrorMessage = "Customer name is required")]
        [StringLength(100, ErrorMessage = "Customer name cannot be more than 100 characters")]
        public string CustomerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Customer phone number is required")]
        [StringLength(15, ErrorMessage = "Phone number cannot be more than 15 characters")]
        public string CustomerPhone { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Food item id must be valid")]
        public int FoodItemId { get; set; }

        [Range(1, 50, ErrorMessage = "Quantity must be between 1 and 50")]
        public int Quantity { get; set; }
    }
}
