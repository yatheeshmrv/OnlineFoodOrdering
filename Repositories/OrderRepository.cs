using FoodOrderAPI.Models;

namespace FoodOrderAPI.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private static readonly List<Order> orders = new List<Order>();

        public async Task<IEnumerable<Order>> GetAllOrdersAsync()
        {
            return await Task.FromResult(orders);
        }

        public async Task<Order?> GetOrderByIdAsync(int id)
        {
            var order = orders.FirstOrDefault(x => x.Id == id);

            return await Task.FromResult(order);
        }

        public async Task<Order> CreateOrderAsync(Order order)
        {
            order.Id = orders.Count == 0 ? 1 : orders.Max(x => x.Id) + 1;

            orders.Add(order);

            return await Task.FromResult(order);
        }

        public async Task<Order?> UpdateOrderStatusAsync(int id, string orderStatus)
        {
            var existingOrder = orders.FirstOrDefault(x => x.Id == id);

            if (existingOrder == null)
            {
                return null;
            }

            existingOrder.OrderStatus = orderStatus;

            return await Task.FromResult(existingOrder);
        }

        public async Task<bool> DeleteOrderAsync(int id)
        {
            var existingOrder = orders.FirstOrDefault(x => x.Id == id);

            if (existingOrder == null)
            {
                return await Task.FromResult(false);
            }

            orders.Remove(existingOrder);

            return await Task.FromResult(true);
        }
    }
}
