using System.ComponentModel.DataAnnotations;

namespace FoodOrderAPI.DTOs
{
    public class CreateOrderDto
    {
        [Required]
        [StringLength(100)]
        public string CustomerName { get; set; } = string.Empty;

        [Required]
        [StringLength(15)]
        public string CustomerPhone { get; set; } = string.Empty;

        [Required]
        [MinLength(1, ErrorMessage = "At least one food item is required")]
        public List<OrderItemRequestDto> Items { get; set; } = [];
    }
}