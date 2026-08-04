using FoodOrderAPI.DTOs;

namespace FoodOrderAPI.Services
{
    // Defines the business operations available
    // for a customer's shopping cart.
    public interface ICartService
    {
        // Returns the logged-in customer's cart.
        // An empty cart is created when one does not exist.
        Task<CartDto> GetCartAsync(string userId);

        // Adds a food item to the customer's cart.
        // Increases the quantity when the item already exists.
        Task<CartDto> AddCartItemAsync(
            AddCartItemDto addCartItemDto,
            string userId);

        // Replaces the quantity of one cart item.
        // Returns null when the item does not exist
        // or belongs to another customer.
        Task<CartDto?> UpdateCartItemQuantityAsync(
            int cartItemId,
            UpdateCartItemQuantityDto quantityDto,
            string userId);

        // Removes one item from the customer's cart.
        Task<bool> RemoveCartItemAsync(
            int cartItemId,
            string userId);

        // Removes every item from the customer's cart.
        Task<bool> ClearCartAsync(string userId);

        // Converts the customer's current cart into an order.
        //
        // The selected saved address is copied into the order
        // as an immutable delivery-address snapshot.
        //
        // Returns a failure response when checkout
        // cannot be completed.
        Task<CreateOrderResponseDto> CheckoutAsync(
            CheckoutDto checkoutDto,
            string userId);
    }
}