using FoodOrderAPI.DTOs;
using FoodOrderAPI.Models;
using FoodOrderAPI.Repositories;
using Microsoft.AspNetCore.Identity;

namespace FoodOrderAPI.Services
{
    // Contains the business logic for shopping-cart operations.
    public class CartService : ICartService
    {
        // Provides access to cart-related database operations.
        private readonly ICartRepository _cartRepository;

        // Provides access to food-item-related database operations.
        private readonly IFoodItemRepository _foodItemRepository;

        // Provides access to order-related database operations.
        private readonly IOrderRepository _orderRepository;

        // Provides access to customer delivery-address operations.
        private readonly IUserAddressRepository
            _userAddressRepository;

        // Provides access to registered Identity users.
        private readonly UserManager<ApplicationUser> _userManager;

        // Receives the required repositories and UserManager
        // through dependency injection.
        public CartService(
            ICartRepository cartRepository,
            IFoodItemRepository foodItemRepository,
            IOrderRepository orderRepository,
            IUserAddressRepository userAddressRepository,
            UserManager<ApplicationUser> userManager)
        {
            // Stores the cart repository.
            _cartRepository = cartRepository;

            // Stores the food-item repository.
            _foodItemRepository = foodItemRepository;

            // Stores the order repository.
            _orderRepository = orderRepository;

            // Stores the saved-address repository.
            _userAddressRepository = userAddressRepository;

            // Stores UserManager for retrieving registered users.
            _userManager = userManager;
        }

        // ---------------------------------------------------------
        // GET CUSTOMER'S CART
        // ---------------------------------------------------------

        // Returns the cart belonging to the logged-in customer.
        public async Task<CartDto> GetCartAsync(
            string userId)
        {
            ValidateUserId(userId);

            // Retrieves the customer's existing cart
            // or creates a new empty cart.
            var cart =
                await _cartRepository.GetOrCreateCartAsync(
                    userId);

            // Converts the Cart entity into a CartDto.
            return MapCart(cart);
        }

        // ---------------------------------------------------------
        // ADD CART ITEM
        // ---------------------------------------------------------

        // Adds a food item to the logged-in customer's cart.
        // Increases the quantity when the same item already exists.
        public async Task<CartDto> AddCartItemAsync(
            AddCartItemDto addCartItemDto,
            string userId)
        {
            ValidateUserId(userId);

            // Prevents a null DTO from reaching the business logic.
            ArgumentNullException.ThrowIfNull(
                addCartItemDto);

            // Keeps direct service calls safe even when they
            // do not pass through FluentValidation.
            if (addCartItemDto.FoodItemId <= 0)
            {
                throw new ArgumentException(
                    "Food item id must be valid.",
                    nameof(addCartItemDto));
            }

            // Ensures that the requested quantity is supported.
            if (addCartItemDto.Quantity < 1 ||
                addCartItemDto.Quantity > 50)
            {
                throw new ArgumentException(
                    "Quantity must be between 1 and 50.",
                    nameof(addCartItemDto));
            }

            // Retrieves the requested food item.
            var foodItem =
                await _foodItemRepository
                    .GetFoodItemByIdAsync(
                        addCartItemDto.FoodItemId);

            // Rejects a food item that does not exist.
            if (foodItem == null)
            {
                throw new ArgumentException(
                    $"Food item " +
                    $"{addCartItemDto.FoodItemId} " +
                    $"was not found.");
            }

            // Rejects a food item that is currently unavailable.
            if (!foodItem.IsAvailable)
            {
                throw new ArgumentException(
                    $"{foodItem.Name} is currently unavailable.");
            }

            // Retrieves or creates the customer's cart.
            var cart =
                await _cartRepository.GetOrCreateCartAsync(
                    userId);

            // Checks whether this food item is already
            // present inside the customer's cart.
            var existingCartItem =
                await _cartRepository
                    .GetCartItemByFoodItemIdAsync(
                        cart.Id,
                        foodItem.Id);

            if (existingCartItem != null)
            {
                // Adding the same item increases its quantity.
                var combinedQuantity =
                    existingCartItem.Quantity +
                    addCartItemDto.Quantity;

                // Prevents the combined quantity from exceeding
                // the supported order limit.
                if (combinedQuantity > 50)
                {
                    throw new ArgumentException(
                        "The total quantity for one food item " +
                        "cannot be more than 50.");
                }

                // Saves the combined quantity.
                var updatedCartItem =
                    await _cartRepository
                        .UpdateCartItemQuantityAsync(
                            existingCartItem.Id,
                            userId,
                            combinedQuantity);

                // Handles an unexpected update failure.
                if (updatedCartItem == null)
                {
                    throw new InvalidOperationException(
                        "The cart item was found but " +
                        "could not be updated.");
                }
            }
            else
            {
                // Creates a new row when this food item
                // is not already present in the cart.
                var newCartItem = new CartItem
                {
                    CartId = cart.Id,
                    FoodItemId = foodItem.Id,
                    Quantity = addCartItemDto.Quantity
                };

                // Saves the new cart item.
                await _cartRepository.AddCartItemAsync(
                    newCartItem);
            }

            // Retrieves the cart again with its latest items.
            var updatedCart =
                await _cartRepository.GetOrCreateCartAsync(
                    userId);

            // Returns the updated cart and recalculated totals.
            return MapCart(updatedCart);
        }

        // ---------------------------------------------------------
        // UPDATE CART ITEM QUANTITY
        // ---------------------------------------------------------

        // Replaces the quantity of one item belonging
        // to the logged-in customer's cart.
        public async Task<CartDto?>
            UpdateCartItemQuantityAsync(
                int cartItemId,
                UpdateCartItemQuantityDto quantityDto,
                string userId)
        {
            ValidateUserId(userId);

            // Prevents a null DTO from reaching the business logic.
            ArgumentNullException.ThrowIfNull(quantityDto);

            // Ensures that a valid cart-item ID was supplied.
            if (cartItemId <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(cartItemId),
                    "Cart item ID must be greater than zero.");
            }

            // Keeps direct service calls safe even when they
            // do not pass through FluentValidation.
            if (quantityDto.Quantity < 1 ||
                quantityDto.Quantity > 50)
            {
                throw new ArgumentException(
                    "Quantity must be between 1 and 50.",
                    nameof(quantityDto));
            }

            // Retrieves the cart item while verifying ownership.
            var existingCartItem =
                await _cartRepository
                    .GetCartItemByIdAndUserIdAsync(
                        cartItemId,
                        userId);

            // Returns null when the item does not exist
            // or belongs to another customer.
            if (existingCartItem == null)
            {
                return null;
            }

            // Handles an unexpected missing FoodItem relationship.
            if (existingCartItem.FoodItem == null)
            {
                throw new InvalidOperationException(
                    "The food item connected to this " +
                    "cart item could not be found.");
            }

            // Prevents changes to an unavailable food item.
            if (!existingCartItem.FoodItem.IsAvailable)
            {
                throw new ArgumentException(
                    $"{existingCartItem.FoodItem.Name} " +
                    "is currently unavailable.");
            }

            // Saves the replacement quantity.
            var updatedCartItem =
                await _cartRepository
                    .UpdateCartItemQuantityAsync(
                        cartItemId,
                        userId,
                        quantityDto.Quantity);

            // Handles an item that disappeared before the update.
            if (updatedCartItem == null)
            {
                return null;
            }

            // Retrieves the complete updated cart.
            var updatedCart =
                await _cartRepository.GetOrCreateCartAsync(
                    userId);

            // Returns the updated cart with recalculated totals.
            return MapCart(updatedCart);
        }

        // ---------------------------------------------------------
        // REMOVE CART ITEM
        // ---------------------------------------------------------

        // Removes one item only when it belongs
        // to the logged-in customer's cart.
        public async Task<bool> RemoveCartItemAsync(
            int cartItemId,
            string userId)
        {
            // Prevents cart access without an authenticated user ID.
            ValidateUserId(userId);

            // Ensures that a valid cart-item ID was supplied.
            if (cartItemId <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(cartItemId),
                    "Cart item ID must be greater than zero.");
            }

            // The repository checks both the item ID and user ID.
            // Therefore, customers cannot remove items
            // belonging to another customer's cart.
            return await _cartRepository.RemoveCartItemAsync(
                cartItemId,
                userId);
        }

        // ---------------------------------------------------------
        // CLEAR CART
        // ---------------------------------------------------------

        // Removes every item from the logged-in customer's cart.
        public async Task<bool> ClearCartAsync(
            string userId)
        {
            // Prevents access without an authenticated user ID.
            ValidateUserId(userId);

            // Ensures that the customer has a cart.
            // A new empty cart is created when necessary.
            await _cartRepository.GetOrCreateCartAsync(
                userId);

            // Removes every CartItem while keeping the Cart record.
            return await _cartRepository.ClearCartAsync(
                userId);
        }

        // ---------------------------------------------------------
        // CHECKOUT
        // ---------------------------------------------------------

        // Converts the logged-in customer's current cart
        // into a new order using their selected saved address.
        public async Task<CreateOrderResponseDto> CheckoutAsync(
            CheckoutDto checkoutDto,
            string userId)
        {
            // Prevents checkout without an authenticated user ID.
            if (string.IsNullOrWhiteSpace(userId))
            {
                return new CreateOrderResponseDto
                {
                    IsSuccess = false,
                    Message = "Authenticated user ID is missing."
                };
            }

            // Prevents a null request from reaching
            // the checkout business logic.
            ArgumentNullException.ThrowIfNull(checkoutDto);

            // Keeps direct service calls safe even when they
            // do not pass through FluentValidation.
            if (checkoutDto.UserAddressId <= 0)
            {
                return new CreateOrderResponseDto
                {
                    IsSuccess = false,
                    Message =
                        "A valid delivery address must be selected."
                };
            }

            // Keeps payment validation active for direct service calls
            // that do not pass through FluentValidation.
            if (!PaymentMethods.IsSupported(
                    checkoutDto.PaymentMethod))
            {
                return new CreateOrderResponseDto
                {
                    IsSuccess = false,
                    Message =
                        "CashOnDelivery is the only supported " +
                        "payment method."
                };
            }

            // Prevents delivery instructions that exceed
            // the database column limit.
            if (checkoutDto.DeliveryInstructions?.Length > 500)
            {
                return new CreateOrderResponseDto
                {
                    IsSuccess = false,
                    Message =
                        "Delivery instructions cannot exceed " +
                        "500 characters."
                };
            }

            // Retrieves the selected address while also checking
            // that it belongs to the authenticated customer.
            var deliveryAddress =
                await _userAddressRepository
                    .GetUserAddressByIdAsync(
                        checkoutDto.UserAddressId,
                        userId);

            // Prevents checkout with an address that does not exist
            // or belongs to another customer.
            if (deliveryAddress == null)
            {
                return new CreateOrderResponseDto
                {
                    IsSuccess = false,
                    Message =
                        "The selected delivery address " +
                        "was not found."
                };
            }

            // Retrieves the registered customer using the user ID
            // obtained from the JWT.
            var user =
                await _userManager.FindByIdAsync(userId);

            // Prevents checkout when the account no longer exists.
            if (user == null)
            {
                return new CreateOrderResponseDto
                {
                    IsSuccess = false,
                    Message =
                        "Authenticated user account was not found."
                };
            }

            // Reads the customer's name and phone number
            // from their registered account.
            var customerName =
                user.FullName?.Trim();

            var customerPhone =
                user.PhoneNumber?.Trim();

            // Prevents checkout when required customer
            // profile information is missing.
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

            // Retrieves the customer's current cart.
            var cart =
                await _cartRepository.GetOrCreateCartAsync(
                    userId);

            // An empty cart cannot be converted into an order.
            if (!cart.CartItems.Any())
            {
                return new CreateOrderResponseDto
                {
                    IsSuccess = false,
                    Message =
                        "The cart is empty. Add at least one item " +
                        "before checkout."
                };
            }

            // Creates the initial order using account details
            // and an immutable snapshot of the selected address.
            var order = new Order
            {
                CustomerName = customerName,
                CustomerPhone = customerPhone,

                // Every new order begins with Pending status.
                OrderStatus = "Pending",

                // Copies the selected payment method into the order.
                PaymentMethod = PaymentMethods.Normalize(
                    checkoutDto.PaymentMethod),

                // Cash-on-delivery remains unpaid until completion.
                PaymentStatus = PaymentStatuses.Pending,

                // Stores the order date in UTC.
                OrderDate = DateTime.UtcNow,

                // Links the order to the authenticated customer.
                UserId = userId,

                // Copies the delivery address into the order.
                // Later changes to the saved address will not
                // modify this historical order.
                DeliveryRecipientName =
                    deliveryAddress.RecipientName,

                DeliveryPhone =
                    deliveryAddress.RecipientPhone,

                DeliveryAddressLine1 =
                    deliveryAddress.AddressLine1,

                DeliveryAddressLine2 =
                    deliveryAddress.AddressLine2,

                DeliveryLandmark =
                    deliveryAddress.Landmark,

                DeliveryCity =
                    deliveryAddress.City,

                DeliveryState =
                    deliveryAddress.State,

                DeliveryPostalCode =
                    deliveryAddress.PostalCode,

                DeliveryInstructions =
                    NormalizeOptionalText(
                        checkoutDto.DeliveryInstructions)
            };

            // Rechecks every food item at checkout time.
            foreach (var cartItem in cart.CartItems)
            {
                // Retrieves the latest food-item information.
                var foodItem =
                    await _foodItemRepository
                        .GetFoodItemByIdAsync(
                            cartItem.FoodItemId);

                // Stops checkout if an item no longer exists.
                if (foodItem == null)
                {
                    return new CreateOrderResponseDto
                    {
                        IsSuccess = false,
                        Message =
                            $"Food item " +
                            $"{cartItem.FoodItemId} " +
                            $"was not found."
                    };
                }

                // Stops checkout if an item is no longer available.
                if (!foodItem.IsAvailable)
                {
                    return new CreateOrderResponseDto
                    {
                        IsSuccess = false,
                        Message =
                            $"{foodItem.Name} is currently unavailable."
                    };
                }

                // Copies the cart quantity and latest food-item
                // price into a new order item.
                order.OrderItems.Add(new OrderItem
                {
                    FoodItemId = foodItem.Id,
                    Quantity = cartItem.Quantity,

                    // Stores the price that applies at checkout.
                    UnitPrice = foodItem.Price
                });
            }

            // Calculates the total using the prices copied
            // into the order items.
            order.TotalAmount = order.OrderItems.Sum(
                item => item.Quantity * item.UnitPrice);

            // Creates the order and clears the cart items.
            // The repository commits both changes using
            // one SaveChangesAsync operation.
            var createdOrder =
                await _orderRepository
                    .CreateOrderFromCartAsync(
                        order,
                        cart.Id);

            // Reloads the order with its related food-item data
            // for the response.
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

            // Returns the successfully created order.
            return new CreateOrderResponseDto
            {
                IsSuccess = true,
                Message = "Order created successfully.",
                Order = MapOrder(savedOrder)
            };
        }

        // ---------------------------------------------------------
        // VALIDATE USER ID
        // ---------------------------------------------------------

        // Ensures that the authenticated user's ID is available.
        private static void ValidateUserId(
            string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException(
                    "User ID is required.",
                    nameof(userId));
            }
        }

        // ---------------------------------------------------------
        // NORMALIZE OPTIONAL TEXT
        // ---------------------------------------------------------

        // Converts empty delivery instructions to null
        // and removes surrounding whitespace.
        private static string? NormalizeOptionalText(
            string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }

        // ---------------------------------------------------------
        // MAP CART
        // ---------------------------------------------------------

        // Converts a Cart entity into a CartDto response
        // and calculates all monetary values.
        private static CartDto MapCart(Cart cart)
        {
            // Converts every CartItem into a CartItemDto.
            var cartItems = cart.CartItems
                .Select(item =>
                {
                    // Reads the food item's current price.
                    var unitPrice =
                        item.FoodItem?.Price ?? 0m;

                    return new CartItemDto
                    {
                        Id = item.Id,
                        FoodItemId = item.FoodItemId,
                        FoodItemName =
    item.FoodItem?.Name ??
    string.Empty,

                        ImageUrl =
    item.FoodItem?.ImageUrl ??
    string.Empty,

                        UnitPrice = unitPrice,
                        Quantity = item.Quantity,
                        IsAvailable =
                            item.FoodItem?.IsAvailable ??
                            false,

                        // Calculates the current item subtotal.
                        Subtotal =
                            unitPrice * item.Quantity
                    };
                })
                .ToList();

            // Creates the complete cart response.
            return new CartDto
            {
                Id = cart.Id,
                Items = cartItems,

                // Adds all calculated item subtotals.
                TotalAmount = cartItems.Sum(
                    item => item.Subtotal)
            };
        }

        // ---------------------------------------------------------
        // MAP ORDER
        // ---------------------------------------------------------

        // Converts an Order entity into an OrderDto response.
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
                PaymentMethod = order.PaymentMethod,
                PaymentStatus = order.PaymentStatus,
                OrderDate = order.OrderDate,

                // Maps the immutable delivery-address snapshot.
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

                // Converts each OrderItem into an OrderItemDto.
                Items = order.OrderItems
                    .Select(item => new OrderItemDto
                    {
                        FoodItemId = item.FoodItemId,
                        FoodItemName =
                            item.FoodItem?.Name ??
                            string.Empty,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice
                    })
                    .ToList()
            };
        }
    }
}