using FoodOrderAPI.Models;

namespace FoodOrderAPI.Services
{
    public interface IOrderService
    {
        // Returns all orders as List<Order>.
        Task<List<Order>> GetAllOrdersAsync();

        // Returns one order by id.
        Task<Order?> GetOrderByIdAsync(int id);

        // Creates a new order.
        Task<Order> CreateOrderAsync(Order order);

        // Updates order status.
        Task<Order?> UpdateOrderStatusAsync(int id, string orderStatus);

        // Deletes order by id.
        Task<bool> DeleteOrderAsync(int id);
    }
}