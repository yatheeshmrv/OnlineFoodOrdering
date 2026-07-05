using System.ComponentModel.DataAnnotations;

namespace FoodOrderAPI.DTOs
{
    // This DTO is used only for updating order status.
    // Example request body from Postman:
    // {
    //     "orderStatus": "Confirmed"
    // }
    public class UpdateOrderStatusDto
    {
        // Required means orderStatus cannot be empty or missing.
        // If user sends empty value, API returns 400 Bad Request.
        [Required(ErrorMessage = "Order status is required")]

        // RegularExpression allows only specific values.
        // Here we allow only:
        // Pending, Confirmed, Cancelled, Delivered
        //
        // ^ means start of value
        // $ means end of value
        // | means OR
        //
        // So this pattern means:
        // orderStatus must be exactly Pending OR Confirmed OR Cancelled OR Delivered
        [RegularExpression(
            "^(Pending|Confirmed|Cancelled|Delivered)$",
            ErrorMessage = "Order status must be Pending, Confirmed, Cancelled, or Delivered"
        )]
        public string OrderStatus { get; set; } = string.Empty;
    }
}