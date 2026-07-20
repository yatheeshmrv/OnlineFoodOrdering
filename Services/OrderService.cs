using FoodOrderAPI.Models;
using FoodOrderAPI.Repositories;

namespace FoodOrderAPI.Services
{
    public class OrderService : IOrderService
    {
        // This field stores the repository object.
        // Service uses repository to perform data operations.
        private readonly IOrderRepository _orderRepository;

        // Constructor injection.
        // ASP.NET Core automatically gives the repository object here using Dependency Injection.
        public OrderService(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        // This method gets all orders from the repository.
        // Repository returns IEnumerable<Order>.
        // We convert it into List<Order> using ToList() to fix the error.
        // Gets all orders from the repository.
        // Sorts the newest orders first and converts the result into a List.
        public async Task<List<Order>> GetAllOrdersAsync()
        {
            IEnumerable<Order> orders =
                await _orderRepository.GetAllOrdersAsync();

            // OrderByDescending requires a property for sorting.
            List<Order> orderList = orders
                .OrderByDescending(order => order.OrderDate)
                .ToList();

            // Every code path now returns a List<Order>.
            return orderList;
        }

        // This method gets one order by id.
        // If the order is not found, repository returns null.
        public async Task<Order?> GetOrderByIdAsync(int id)
        {
            Order? order = await _orderRepository.GetOrderByIdAsync(id);

            return order;
        }

        // This method creates a new order.
        // The order object is sent from controller to service,
        // then service sends it to repository to save it.
        public async Task<Order> CreateOrderAsync(Order order)
        {
            // If order status is empty, we set a default status.
            // This makes sure every new order has some status.
            if (string.IsNullOrWhiteSpace(order.OrderStatus))
            {
                order.OrderStatus = "Pending";
            }

            Order createdOrder = await _orderRepository.CreateOrderAsync(order);

            return createdOrder;
        }

        // This method updates only the order status.
        // Example status values: Pending, Preparing, Delivered, Cancelled.
        public async Task<Order?> UpdateOrderStatusAsync(int id, string orderStatus)
        {
            Order? updatedOrder = await _orderRepository.UpdateOrderStatusAsync(id, orderStatus);

            return updatedOrder;
        }

        // This method deletes an order by id.
        // It returns true if deleted successfully.
        // It returns false if order is not found.
        public async Task<bool> DeleteOrderAsync(int id)
        {
            bool isDeleted = await _orderRepository.DeleteOrderAsync(id);

            return isDeleted;
        }
    }
}