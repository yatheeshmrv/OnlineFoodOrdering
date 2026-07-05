using FoodOrderAPI.Models;

namespace FoodOrderAPI.Repositories
{
    public interface IOrderRepository
    {
        Task<IEnumerable<Order>> GetAllOrdersAsync();

        Task<Order?> GetOrderByIdAsync(int id);

        Task<Order> CreateOrderAsync(Order order);

        Task<Order?> UpdateOrderStatusAsync(int id, string orderStatus);

        Task<bool> DeleteOrderAsync(int id);
    }
}
