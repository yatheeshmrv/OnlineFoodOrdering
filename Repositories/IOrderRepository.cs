using FoodOrderAPI.Models;

namespace FoodOrderAPI.Repositories
{
    public interface IOrderRepository
    {
        // Returns every order. Used by Admin.
        Task<IEnumerable<Order>> GetAllOrdersAsync();

        // Returns a specific order by its ID.
        Task<Order?> GetOrderByIdAsync(int id);

        // Returns only the orders placed by a particular customer.
        Task<IEnumerable<Order>> GetOrdersByUserIdAsync(string userId);
        Task<Order?> GetOrderByIdAndUserIdAsync(int id,string userId);

        // Creates a new order.
        Task<Order> CreateOrderAsync(Order order);

        // Updates the status of an existing order.
        Task<Order?> UpdateOrderStatusAsync(
            int id,
            string orderStatus);

        // Deletes an order.
        Task<bool> DeleteOrderAsync(int id);
    }
}