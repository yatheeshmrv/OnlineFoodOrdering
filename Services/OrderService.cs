using FoodOrderAPI.DTOs;
using FoodOrderAPI.Models;
using FoodOrderAPI.Repositories;
using Microsoft.AspNetCore.Identity;

namespace FoodOrderAPI.Services
{
    // Contains the business logic for order-related operations.
    public class OrderService : IOrderService
    {
        // Provides access to order-related database operations.
        private readonly IOrderRepository _orderRepository;

        // Provides access to food-item-related database operations.
        private readonly IFoodItemRepository _foodItemRepository;

        // Provides access to registered Identity users.
        private readonly UserManager<ApplicationUser> _userManager;

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

        // Receives the repositories and UserManager through
        // dependency injection.
        public OrderService(
            IOrderRepository orderRepository,
            IFoodItemRepository foodItemRepository,
            UserManager<ApplicationUser> userManager)
        {
            // Stores the order repository.
            _orderRepository = orderRepository;

            // Stores the food item repository.
            _foodItemRepository = foodItemRepository;

            // Stores UserManager for retrieving registered users.
            _userManager = userManager;
        }

        // ---------------------------------------------------------
        // GET ALL ORDERS
        // ---------------------------------------------------------

        // Returns every order in the application.
        // This method is intended for Admin users.
        public async Task<List<OrderDto>> GetAllOrdersAsync()
        {
            // Retrieves all orders from the database.
            var orders =
                await _orderRepository.GetAllOrdersAsync();

            // Converts the Order entities into OrderDto objects.
            return orders
                .Select(MapOrder)
                .ToList();
        }

        // ---------------------------------------------------------
        // GET ORDER BY ID
        // ---------------------------------------------------------

        // Returns one order using its order ID.
        // This method is intended for Admin users.
        public async Task<OrderDto?> GetOrderByIdAsync(int id)
        {
            // Searches for the order in the database.
            var order =
                await _orderRepository.GetOrderByIdAsync(id);

            // Returns null when the order does not exist.
            // Otherwise, converts it into an OrderDto.
            return order == null
                ? null
                : MapOrder(order);
        }

        // ---------------------------------------------------------
        // GET LOGGED-IN CUSTOMER'S ORDERS
        // ---------------------------------------------------------

        // Returns only the orders belonging to the
        // currently logged-in customer.
        public async Task<IEnumerable<OrderDto>> GetMyOrdersAsync(
            string userId)
        {
            // Prevents searching without an authenticated user ID.
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException(
                    "User ID is required.",
                    nameof(userId));
            }

            // Retrieves orders matching the authenticated user's ID.
            var orders =
                await _orderRepository.GetOrdersByUserIdAsync(
                    userId);

            // Converts the matching orders into OrderDto objects.
            return orders
                .Select(MapOrder)
                .ToList();
        }

        // ---------------------------------------------------------
        // GET LOGGED-IN CUSTOMER'S ORDER BY ID
        // ---------------------------------------------------------

        // Returns one order only when it belongs to the
        // currently logged-in customer.
        public async Task<OrderDto?> GetMyOrderByIdAsync(
            int id,
            string userId)
        {
            // Ensures that the order ID is valid.
            if (id <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(id),
                    "Order ID must be greater than zero.");
            }

            // Ensures that an authenticated user ID was provided.
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException(
                    "User ID is required.",
                    nameof(userId));
            }

            // Both the order ID and user ID must match.
            // This prevents customers from viewing orders
            // belonging to another customer.
            var order =
                await _orderRepository.GetOrderByIdAndUserIdAsync(
                    id,
                    userId);

            // Returns null when the order does not exist
            // or does not belong to the logged-in customer.
            return order == null
                ? null
                : MapOrder(order);
        }

        // ---------------------------------------------------------
        // CREATE ORDER
        // ---------------------------------------------------------

        // Creates an order and links it to the
        // currently logged-in customer.
        public async Task<CreateOrderResponseDto> CreateOrderAsync(
            CreateOrderDto createOrderDto,
            string userId)
        {
            // Ensures that the authenticated user's ID is available.
            if (string.IsNullOrWhiteSpace(userId))
            {
                return new CreateOrderResponseDto
                {
                    IsSuccess = false,
                    Message = "Authenticated user ID is missing."
                };
            }

            // Retrieves the registered customer using the user ID
            // obtained from the JWT.
            var user =
                await _userManager.FindByIdAsync(userId);

            // Prevents order creation when the account
            // no longer exists.
            if (user == null)
            {
                return new CreateOrderResponseDto
                {
                    IsSuccess = false,
                    Message =
                        "Authenticated user account was not found."
                };
            }

            // Reads the customer's details from their account.
            // These values no longer come from the order JSON.
            var customerName =
                user.FullName?.Trim();

            var customerPhone =
                user.PhoneNumber?.Trim();

            // Older accounts may not have a saved name or phone.
            if (string.IsNullOrWhiteSpace(customerName) ||
                string.IsNullOrWhiteSpace(customerPhone))
            {
                return new CreateOrderResponseDto
                {
                    IsSuccess = false,
                    Message =
                        "Customer profile is incomplete. " +
                        "Full name and phone number are required."
                };
            }

            // Creates the initial Order entity.
            var order = new Order
            {
                // Customer details come from the registered account.
                CustomerName = customerName,
                CustomerPhone = customerPhone,

                // Every new order begins with Pending status.
                OrderStatus = "Pending",

                // Stores the order date in UTC.
                OrderDate = DateTime.UtcNow,

                // Links the order to the authenticated customer.
                UserId = userId
            };

            // Processes every food item requested by the customer.
            foreach (var requestedItem in createOrderDto.Items)
            {
                // Retrieves the requested food item.
                var foodItem =
                    await _foodItemRepository.GetFoodItemByIdAsync(
                        requestedItem.FoodItemId);

                // Stops order creation when the food item
                // does not exist.
                if (foodItem == null)
                {
                    return new CreateOrderResponseDto
                    {
                        IsSuccess = false,
                        Message =
                            $"Food item " +
                            $"{requestedItem.FoodItemId} " +
                            $"was not found."
                    };
                }

                // Stops order creation when the food item
                // is currently unavailable.
                if (!foodItem.IsAvailable)
                {
                    return new CreateOrderResponseDto
                    {
                        IsSuccess = false,
                        Message =
                            $"{foodItem.Name} is currently unavailable."
                    };
                }

                // Adds the validated food item to the order.
                order.OrderItems.Add(new OrderItem
                {
                    FoodItemId = foodItem.Id,
                    Quantity = requestedItem.Quantity,

                    // Stores the price at the time of ordering.
                    UnitPrice = foodItem.Price
                });
            }

            // Calculates the total amount of the complete order.
            order.TotalAmount = order.OrderItems.Sum(
                item => item.Quantity * item.UnitPrice);

            // Saves the order and its order items.
            var createdOrder =
                await _orderRepository.CreateOrderAsync(order);

            // Retrieves the saved order with its related data.
            var savedOrder =
                await _orderRepository.GetOrderByIdAsync(
                    createdOrder.Id);

            // Handles an unexpected failure to retrieve
            // the newly created order.
            if (savedOrder == null)
            {
                throw new InvalidOperationException(
                    "The order was created but could not be retrieved.");
            }

            // Returns the successful order response.
            return new CreateOrderResponseDto
            {
                IsSuccess = true,
                Message = "Order created successfully.",
                Order = MapOrder(savedOrder)
            };
        }

        // ---------------------------------------------------------
        // UPDATE ORDER STATUS
        // ---------------------------------------------------------

        // Updates an existing order's status.
        public async Task<OrderDto?> UpdateOrderStatusAsync(
            int id,
            UpdateOrderStatusDto statusDto)
        {
            // Removes unnecessary spaces from the requested status.
            var requestedStatus =
                statusDto.OrderStatus?.Trim();

            // Ensures that the requested status is supported.
            if (string.IsNullOrWhiteSpace(requestedStatus) ||
                !ValidStatuses.Contains(requestedStatus))
            {
                throw new ArgumentException(
                    "Invalid order status. Allowed values are " +
                    "Pending, Confirmed, Preparing, " +
                    "Out for Delivery, Delivered, or Cancelled.");
            }

            // Finds the standard capitalization stored
            // in the ValidStatuses collection.
            var normalizedStatus = ValidStatuses.First(
                status => status.Equals(
                    requestedStatus,
                    StringComparison.OrdinalIgnoreCase));

            // Updates the status in the database.
            var updatedOrder =
                await _orderRepository.UpdateOrderStatusAsync(
                    id,
                    normalizedStatus);

            // Returns null if the order was not found.
            // Otherwise, converts it into an OrderDto.
            return updatedOrder == null
                ? null
                : MapOrder(updatedOrder);
        }

        // ---------------------------------------------------------
        // DELETE ORDER
        // ---------------------------------------------------------

        // Deletes an order using its order ID.
        public Task<bool> DeleteOrderAsync(int id)
        {
            // Returns true when deletion succeeds.
            // Returns false when the order does not exist.
            return _orderRepository.DeleteOrderAsync(id);
        }

        // ---------------------------------------------------------
        // MAP ORDER
        // ---------------------------------------------------------

        // Converts an Order entity into an OrderDto response.
        private static OrderDto MapOrder(Order order)
        {
            return new OrderDto
            {
                // Maps the main order details.
                Id = order.Id,
                CustomerName = order.CustomerName,
                CustomerPhone = order.CustomerPhone,
                TotalAmount = order.TotalAmount,
                OrderStatus = order.OrderStatus,
                OrderDate = order.OrderDate,

                // Converts every OrderItem into an OrderItemDto.
                Items = order.OrderItems
                    .Select(item => new OrderItemDto
                    {
                        FoodItemId = item.FoodItemId,

                        // Uses an empty value if FoodItem
                        // was not loaded.
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