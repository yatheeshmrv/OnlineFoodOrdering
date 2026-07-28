namespace FoodOrderAPI.DTOs
{
    // This DTO is used as a response when creating an order.
    // We are creating this because order creation can have different results:
    // 1. Order created successfully
    // 2. Food item not found
    // 3. Food item is not available
    public class CreateOrderResponseDto
    {
        // This tells whether the order creation was successful or not.
        // true  = order created
        // false = order not created
        public bool IsSuccess { get; set; }

        // This gives a clear message to Postman/user.
        // Example:
        // "Order created successfully"
        // "Food item not found"
        // "Food item is currently unavailable"
        public string Message { get; set; } = string.Empty;

        // This will contain order details only when order is created successfully.
        // If order is not created, this can be null.
        public OrderDto? Order { get; set; }
    }
}
