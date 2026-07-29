using FoodOrderAPI.Models;

namespace FoodOrderAPI.Repositories
{
    // Defines the database operations required
    // for managing customer shopping carts.
    public interface ICartRepository
    {
        // Returns the cart belonging to the specified customer.
        // Creates an empty cart when the customer does not have one yet.
        Task<Cart> GetOrCreateCartAsync(string userId);

        // Returns a specific cart item only when it belongs
        // to the specified logged-in customer.
        Task<CartItem?> GetCartItemByIdAndUserIdAsync(
            int cartItemId,
            string userId);

        // Finds an existing food item inside a particular cart.
        // Used to increase its quantity instead of adding a duplicate row.
        Task<CartItem?> GetCartItemByFoodItemIdAsync(
            int cartId,
            int foodItemId);

        // Adds a new food item to a cart.
        Task<CartItem> AddCartItemAsync(CartItem cartItem);

        // Replaces the quantity of an existing cart item.
        // The user ID prevents customers from updating another user's cart.
        Task<CartItem?> UpdateCartItemQuantityAsync(
            int cartItemId,
            string userId,
            int quantity);

        // Removes one item only when it belongs
        // to the specified customer's cart.
        Task<bool> RemoveCartItemAsync(
            int cartItemId,
            string userId);

        // Removes every item from the specified customer's cart.
        Task<bool> ClearCartAsync(string userId);
    }
}