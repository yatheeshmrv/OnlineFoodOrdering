using FoodOrderAPI.DTOs;
using FoodOrderAPI.Models;
using FoodOrderAPI.Repositories;

namespace FoodOrderAPI.Services
{
    // OrderService contains the business logic for orders.
    // It connects Controller with Repository.
    public class OrderService : IOrderService
    {
        // Order repository is used to store, get, update, and delete orders.
        private readonly IOrderRepository _orderRepository;

        // Food item repository is used to check whether the selected food item exists.
        // It is also used to get food item price for total amount calculation.
        private readonly IFoodItemRepository _foodItemRepository;

        // Constructor Injection:
        // ASP.NET Core will inject OrderRepository and FoodItemRepository here.
        public OrderService(
            IOrderRepository orderRepository,
            IFoodItemRepository foodItemRepository)
        {
            _orderRepository = orderRepository;
            _foodItemRepository = foodItemRepository;
        }

        // Gets all orders from repository and converts them from Order model to OrderDto.
        public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync()
        {
            var orders = await _orderRepository.GetAllOrdersAsync();

            return orders.Select(order => MapToDto(order));
        }

        // Gets one order by id.
        // If order is not found, returns null.
        public async Task<OrderDto?> GetOrderByIdAsync(int id)
        {
            var order = await _orderRepository.GetOrderByIdAsync(id);

            if (order == null)
            {
                return null;
            }

            return MapToDto(order);
        }

        // Creates a new order.
        // This method also calculates TotalAmount.
        // Creates a new order.
        // This method handles 3 cases:
        // 1. Food item does not exist
        // 2. Food item exists but is not available
        // 3. Food item exists and is available, so order is created
        public async Task<CreateOrderResponseDto> CreateOrderAsync(CreateOrderDto createOrderDto)
        {
            // First, get the food item using FoodItemId from the request.
            // Example: If user sends foodItemId = 1, we check whether food item 1 exists.
            var foodItem = await _foodItemRepository.GetFoodItemByIdAsync(createOrderDto.FoodItemId);

            // If food item is not found, we should not create the order.
            if (foodItem == null)
            {
                return new CreateOrderResponseDto
                {
                    IsSuccess = false,
                    Message = "Food item not found",
                    Order = null
                };
            }

            // If food item is found but not available, we should not create the order.
            if (foodItem.IsAvailable == false)
            {
                return new CreateOrderResponseDto
                {
                    IsSuccess = false,
                    Message = "Food item is currently unavailable",
                    Order = null
                };
            }

            // Create Order model from CreateOrderDto.
            // User sends only customer details, foodItemId, and quantity.
            // API calculates TotalAmount, OrderStatus, and OrderDate.
            var order = new Order
            {
                CustomerName = createOrderDto.CustomerName,
                CustomerPhone = createOrderDto.CustomerPhone,
                FoodItemId = createOrderDto.FoodItemId,
                Quantity = createOrderDto.Quantity,

                // Total amount = food item price × quantity
                TotalAmount = foodItem.Price * createOrderDto.Quantity,

                // New order status will be Pending by default.
                OrderStatus = "Pending",

                // Store current date and time of order creation.
                OrderDate = DateTime.Now
            };

            // Save order using OrderRepository.
            var createdOrder = await _orderRepository.CreateOrderAsync(order);

            // Return success response with created order details.
            return new CreateOrderResponseDto
            {
                IsSuccess = true,
                Message = "Order created successfully",
                Order = MapToDto(createdOrder)
            };
        }

        // Updates only order status.
        public async Task<OrderDto?> UpdateOrderStatusAsync(
            int id,
            UpdateOrderStatusDto updateOrderStatusDto)
        {
            var updatedOrder = await _orderRepository.UpdateOrderStatusAsync(
                id,
                updateOrderStatusDto.OrderStatus);

            if (updatedOrder == null)
            {
                return null;
            }

            return MapToDto(updatedOrder);
        }

        // Deletes order by id.
        public async Task<bool> DeleteOrderAsync(int id)
        {
            return await _orderRepository.DeleteOrderAsync(id);
        }

        // Private helper method.
        // Converts Order model to OrderDto.
        // This prevents exposing model directly to controller.
        private OrderDto MapToDto(Order order)
        {
            return new OrderDto
            {
                Id = order.Id,
                CustomerName = order.CustomerName,
                CustomerPhone = order.CustomerPhone,
                FoodItemId = order.FoodItemId,
                Quantity = order.Quantity,
                TotalAmount = order.TotalAmount,
                OrderStatus = order.OrderStatus,
                OrderDate = order.OrderDate
            };
        }
    }
}
