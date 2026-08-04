using FoodOrderAPI.DTOs;

namespace FoodOrderAPI.Services
{
    // Defines the business operations available
    // for customer and administrator order management.
    public interface IOrderService
    {
        // Returns every order in the application.
        // Intended for Admin users.
        Task<List<OrderDto>> GetAllOrdersAsync();

        // Returns one order using its order ID.
        // Intended for Admin users.
        Task<OrderDto?> GetOrderByIdAsync(int id);

        // Returns all orders belonging to the
        // logged-in customer.
        Task<IEnumerable<OrderDto>> GetMyOrdersAsync(
            string userId);

        // Returns one order only when it belongs
        // to the logged-in customer.
        Task<OrderDto?> GetMyOrderByIdAsync(
            int id,
            string userId);

        // Updates the status of an existing order.
        // Intended for Admin users.
        Task<OrderDto?> UpdateOrderStatusAsync(
            int id,
            UpdateOrderStatusDto statusDto);
    }
}