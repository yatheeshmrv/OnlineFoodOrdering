namespace FoodOrderAPI.DTOs
{
    // Contains the food items requested by the authenticated customer.
    // Customer details are obtained from the logged-in user account.
    public class CreateOrderDto
    {
        // List of food items included in the order.
        public List<OrderItemRequestDto> Items { get; set; } = [];
    }
}