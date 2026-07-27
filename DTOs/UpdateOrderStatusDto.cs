namespace FoodOrderAPI.DTOs
{
    // Contains the new status when updating an order.
    public class UpdateOrderStatusDto
    {
        // Stores the requested order status.
        public string OrderStatus { get; set; } = string.Empty;
    }
}