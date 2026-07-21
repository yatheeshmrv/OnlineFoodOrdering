using FoodOrderAPI.DTOs;
using FoodOrderAPI.Models;
using FoodOrderAPI.Repositories;

namespace FoodOrderAPI.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IFoodItemRepository _foodItemRepository;

        private static readonly HashSet<string> ValidStatuses =
            new(StringComparer.OrdinalIgnoreCase)
            {
                "Pending",
                "Confirmed",
                "Preparing",
                "Out for Delivery",
                "Delivered",
                "Cancelled"
            };

        public OrderService(
            IOrderRepository orderRepository,
            IFoodItemRepository foodItemRepository)
        {
            _orderRepository = orderRepository;
            _foodItemRepository = foodItemRepository;
        }

        public async Task<List<OrderDto>> GetAllOrdersAsync()
        {
            var orders = await _orderRepository.GetAllOrdersAsync();

            return orders
                .Select(MapOrder)
                .ToList();
        }

        public async Task<OrderDto?> GetOrderByIdAsync(int id)
        {
            var order = await _orderRepository.GetOrderByIdAsync(id);

            return order == null
                ? null
                : MapOrder(order);
        }

        public async Task<CreateOrderResponseDto> CreateOrderAsync(
            CreateOrderDto createOrderDto)
        {
            var order = new Order
            {
                CustomerName = createOrderDto.CustomerName.Trim(),
                CustomerPhone = createOrderDto.CustomerPhone.Trim(),
                OrderStatus = "Pending",
                OrderDate = DateTime.UtcNow
            };

            foreach (var requestedItem in createOrderDto.Items)
            {
                var foodItem = await _foodItemRepository
                    .GetFoodItemByIdAsync(requestedItem.FoodItemId);

                if (foodItem == null)
                {
                    return new CreateOrderResponseDto
                    {
                        IsSuccess = false,
                        Message =
                            $"Food item {requestedItem.FoodItemId} was not found."
                    };
                }

                if (!foodItem.IsAvailable)
                {
                    return new CreateOrderResponseDto
                    {
                        IsSuccess = false,
                        Message =
                            $"{foodItem.Name} is currently unavailable."
                    };
                }

                order.OrderItems.Add(new OrderItem
                {
                    FoodItemId = foodItem.Id,
                    Quantity = requestedItem.Quantity,
                    UnitPrice = foodItem.Price
                });
            }

            order.TotalAmount = order.OrderItems.Sum(
                item => item.Quantity * item.UnitPrice);

            var createdOrder =
                await _orderRepository.CreateOrderAsync(order);

            var savedOrder =
                await _orderRepository.GetOrderByIdAsync(createdOrder.Id);

            if (savedOrder == null)
            {
                throw new InvalidOperationException(
                    "The order was created but could not be retrieved.");
            }

            return new CreateOrderResponseDto
            {
                IsSuccess = true,
                Message = "Order created successfully.",
                Order = MapOrder(savedOrder)
            };
        }

        public async Task<OrderDto?> UpdateOrderStatusAsync(
            int id,
            UpdateOrderStatusDto statusDto)
        {
            var requestedStatus = statusDto.OrderStatus?.Trim();

            if (string.IsNullOrWhiteSpace(requestedStatus) ||
                !ValidStatuses.Contains(requestedStatus))
            {
                throw new ArgumentException(
                    "Invalid order status. Allowed values are Pending, " +
                    "Confirmed, Preparing, Out for Delivery, Delivered, " +
                    "or Cancelled.");
            }

            // Stores the status using its standard capitalization.
            var normalizedStatus = ValidStatuses.First(
                status => status.Equals(
                    requestedStatus,
                    StringComparison.OrdinalIgnoreCase));

            var updatedOrder =
                await _orderRepository.UpdateOrderStatusAsync(
                    id,
                    normalizedStatus);

            return updatedOrder == null
                ? null
                : MapOrder(updatedOrder);
        }

        public Task<bool> DeleteOrderAsync(int id)
        {
            return _orderRepository.DeleteOrderAsync(id);
        }

        private static OrderDto MapOrder(Order order)
        {
            return new OrderDto
            {
                Id = order.Id,
                CustomerName = order.CustomerName,
                CustomerPhone = order.CustomerPhone,
                TotalAmount = order.TotalAmount,
                OrderStatus = order.OrderStatus,
                OrderDate = order.OrderDate,

                Items = order.OrderItems
                    .Select(item => new OrderItemDto
                    {
                        FoodItemId = item.FoodItemId,
                        FoodItemName =
                            item.FoodItem?.Name ?? string.Empty,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice
                    })
                    .ToList()
            };
        }
    }
}