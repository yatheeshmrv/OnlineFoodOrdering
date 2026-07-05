using FoodOrderAPI.DTOs;

namespace FoodOrderAPI.Services
{
    // This interface defines what operations OrderService must provide.
    // Controller will depend on this interface, not directly on OrderService class.
    public interface IOrderService
    {
        // Gets all orders and returns them as OrderDto.
        // We return DTO because controller should not expose the Order model directly.
        Task<IEnumerable<OrderDto>> GetAllOrdersAsync();

        // Gets one order by id.
        // The ? means it can return null if order is not found.
        Task<OrderDto?> GetOrderByIdAsync(int id);

        // Creates a new order and returns a clear response.
        // Response can say:
        // 1. Order created successfully
        // 2. Food item not found
        // 3. Food item is currently unavailable
        Task<CreateOrderResponseDto> CreateOrderAsync(CreateOrderDto createOrderDto);

        // Updates only the order status.
        // Example status: Pending, Confirmed, Cancelled.
        Task<OrderDto?> UpdateOrderStatusAsync(int id, UpdateOrderStatusDto updateOrderStatusDto);

        // Deletes order by id.
        // Returns true if deleted, false if order not found.
        Task<bool> DeleteOrderAsync(int id);
    }
}
