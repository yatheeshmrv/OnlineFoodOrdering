using FoodOrderAPI.DTOs;
using FoodOrderAPI.Models;
using FoodOrderAPI.Repositories;

namespace FoodOrderAPI.Services
{
    // Contains the business logic for order-related operations.
    public class OrderService : IOrderService
    {
        // Provides access to order-related database operations.
        private readonly IOrderRepository _orderRepository;

        // Contains every order status accepted by the application.
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

        // Receives the order repository through dependency injection.
        public OrderService(
            IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        // ---------------------------------------------------------
        // GET ALL ORDERS
        // ---------------------------------------------------------

        // Returns every order in the application.
        // Intended for Admin users.
        public async Task<List<OrderDto>> GetAllOrdersAsync()
        {
            var orders =
                await _orderRepository.GetAllOrdersAsync();

            return orders
                .Select(MapOrder)
                .ToList();
        }

        // ---------------------------------------------------------
        // GET ORDER BY ID
        // ---------------------------------------------------------

        // Returns one order using its order ID.
        // Intended for Admin users.
        public async Task<OrderDto?> GetOrderByIdAsync(
            int id)
        {
            var order =
                await _orderRepository.GetOrderByIdAsync(id);

            return order == null
                ? null
                : MapOrder(order);
        }

        // ---------------------------------------------------------
        // GET LOGGED-IN CUSTOMER'S ORDERS
        // ---------------------------------------------------------

        // Returns only the orders belonging to the
        // currently logged-in customer.
        public async Task<IEnumerable<OrderDto>>
            GetMyOrdersAsync(
                string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException(
                    "User ID is required.",
                    nameof(userId));
            }

            var orders =
                await _orderRepository
                    .GetOrdersByUserIdAsync(userId);

            return orders
                .Select(MapOrder)
                .ToList();
        }

        // ---------------------------------------------------------
        // GET LOGGED-IN CUSTOMER'S ORDER BY ID
        // ---------------------------------------------------------

        // Returns one order only when it belongs to the
        // currently logged-in customer.
        public async Task<OrderDto?>
            GetMyOrderByIdAsync(
                int id,
                string userId)
        {
            if (id <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(id),
                    "Order ID must be greater than zero.");
            }

            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException(
                    "User ID is required.",
                    nameof(userId));
            }

            var order =
                await _orderRepository
                    .GetOrderByIdAndUserIdAsync(
                        id,
                        userId);

            return order == null
                ? null
                : MapOrder(order);
        }

        // ---------------------------------------------------------
        // UPDATE ORDER STATUS
        // ---------------------------------------------------------

        // Updates an existing order's status.
        // Intended for Admin users.
        public async Task<OrderDto?>
            UpdateOrderStatusAsync(
                int id,
                UpdateOrderStatusDto statusDto)
        {
            ArgumentNullException.ThrowIfNull(statusDto);

            var requestedStatus =
                statusDto.OrderStatus?.Trim();

            if (string.IsNullOrWhiteSpace(requestedStatus) ||
                !ValidStatuses.Contains(requestedStatus))
            {
                throw new ArgumentException(
                    "Invalid order status. Allowed values are " +
                    "Pending, Confirmed, Preparing, " +
                    "Out for Delivery, Delivered, or Cancelled.");
            }

            var normalizedStatus =
                ValidStatuses.First(status =>
                    status.Equals(
                        requestedStatus,
                        StringComparison.OrdinalIgnoreCase));

            var updatedOrder =
                await _orderRepository
                    .UpdateOrderStatusAsync(
                        id,
                        normalizedStatus);

            return updatedOrder == null
                ? null
                : MapOrder(updatedOrder);
        }

        // ---------------------------------------------------------
        // MAP ORDER
        // ---------------------------------------------------------

        // Converts an Order entity into an OrderDto response.
        private static OrderDto MapOrder(
            Order order)
        {
            return new OrderDto
            {
                Id = order.Id,
                CustomerName = order.CustomerName,
                CustomerPhone = order.CustomerPhone,
                TotalAmount = order.TotalAmount,
                OrderStatus = order.OrderStatus,
                PaymentMethod = order.PaymentMethod,
                PaymentStatus = order.PaymentStatus,
                OrderDate = order.OrderDate,

                // Immutable delivery-address snapshot.
                DeliveryRecipientName =
                    order.DeliveryRecipientName,

                DeliveryPhone =
                    order.DeliveryPhone,

                DeliveryAddressLine1 =
                    order.DeliveryAddressLine1,

                DeliveryAddressLine2 =
                    order.DeliveryAddressLine2,

                DeliveryLandmark =
                    order.DeliveryLandmark,

                DeliveryCity =
                    order.DeliveryCity,

                DeliveryState =
                    order.DeliveryState,

                DeliveryPostalCode =
                    order.DeliveryPostalCode,

                DeliveryInstructions =
                    order.DeliveryInstructions,

                Items = order.OrderItems
                    .Select(item => new OrderItemDto
                    {
                        FoodItemId = item.FoodItemId,

                        FoodItemName =
                        item.FoodItem?.Name ??
                        string.Empty,

                        ImageUrl =
                        item.FoodItem?.ImageUrl ??
                        string.Empty,

                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice
                    })
                    .ToList()
            };
        }
    }
}