using System.ComponentModel.DataAnnotations;

namespace FoodOrderAPI.DTOs
{
    public class OrderItemRequestDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "Food item id must be valid")]
        public int FoodItemId { get; set; }

        [Range(1, 50, ErrorMessage = "Quantity must be between 1 and 50")]
        public int Quantity { get; set; }
    }
}
