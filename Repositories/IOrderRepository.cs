using FoodOrderAPI.Models;

namespace FoodOrderAPI.Repositories
{
    // Defines the database operations available for orders.
    public interface IOrderRepository
    {
        // Returns every order. Used by Admin.
        Task<IEnumerable<Order>> GetAllOrdersAsync();

        // Returns a specific order by its ID.
        Task<Order?> GetOrderByIdAsync(int id);

        // Returns only the orders placed by a particular customer.
        Task<IEnumerable<Order>> GetOrdersByUserIdAsync(
            string userId);

        // Returns an order only when it belongs to the specified customer.
        Task<Order?> GetOrderByIdAndUserIdAsync(
            int id,
            string userId);

        // Creates a new order.
        Task<Order> CreateOrderAsync(Order order);

        // Creates an order from a shopping cart and clears the cart items.
        // Both operations will be committed using one SaveChangesAsync call.
        Task<Order> CreateOrderFromCartAsync(
            Order order,
            int cartId);

        // Updates the status of an existing order.
        Task<Order?> UpdateOrderStatusAsync(
            int id,
            string orderStatus);

        // Deletes an order.
        Task<bool> DeleteOrderAsync(int id);
    }
}