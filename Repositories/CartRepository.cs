using FoodOrderAPI.Data;
using FoodOrderAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace FoodOrderAPI.Repositories
{
    // Handles database operations for customer shopping carts.
    public class CartRepository : ICartRepository
    {
        // Used to communicate with the SQL Server database.
        private readonly ApplicationDbContext _context;

        // Receives ApplicationDbContext through dependency injection.
        public CartRepository(ApplicationDbContext context)
        {
            // Stores the injected database context.
            _context = context;
        }

        // Returns the cart belonging to the specified customer.
        // Creates an empty cart when the customer does not have one.
        public async Task<Cart> GetOrCreateCartAsync(
            string userId)
        {
            // Loads the cart together with its food items.
            var existingCart = await _context.Carts
                .Include(cart => cart.CartItems)
                .ThenInclude(cartItem => cartItem.FoodItem)
                .AsNoTracking()
                .FirstOrDefaultAsync(cart =>
                    cart.UserId == userId);

            // Returns the existing cart when one is found.
            if (existingCart != null)
            {
                return existingCart;
            }

            // Creates the customer's first empty cart.
            var newCart = new Cart
            {
                UserId = userId
            };

            // Adds the cart to EF Core tracking.
            await _context.Carts.AddAsync(newCart);

            // Saves the new cart to the database.
            await _context.SaveChangesAsync();

            // Returns the newly created empty cart.
            return newCart;
        }

        // Returns a cart item only when it belongs
        // to the specified customer.
        public async Task<CartItem?>
            GetCartItemByIdAndUserIdAsync(
                int cartItemId,
                string userId)
        {
            return await _context.CartItems
                .Include(cartItem => cartItem.FoodItem)
                .AsNoTracking()
                .FirstOrDefaultAsync(cartItem =>
                    cartItem.Id == cartItemId &&
                    cartItem.Cart != null &&
                    cartItem.Cart.UserId == userId);
        }

        // Finds a food item that is already present
        // inside the specified cart.
        public async Task<CartItem?>
            GetCartItemByFoodItemIdAsync(
                int cartId,
                int foodItemId)
        {
            return await _context.CartItems
                .Include(cartItem => cartItem.FoodItem)
                .AsNoTracking()
                .FirstOrDefaultAsync(cartItem =>
                    cartItem.CartId == cartId &&
                    cartItem.FoodItemId == foodItemId);
        }

        // Adds a new food item to the cart.
        public async Task<CartItem>
            AddCartItemAsync(CartItem cartItem)
        {
            // The foreign-key IDs already identify the existing
            // Cart and FoodItem records.
            //
            // Setting navigation properties to null prevents
            // EF Core from attempting to insert related records.
            cartItem.Cart = null;
            cartItem.FoodItem = null;

            // Adds the new cart item to EF Core tracking.
            await _context.CartItems.AddAsync(cartItem);

            // Saves the new cart item to the database.
            await _context.SaveChangesAsync();

            // Reloads the cart item with its FoodItem information.
            return await _context.CartItems
                .Include(item => item.FoodItem)
                .AsNoTracking()
                .FirstAsync(item => item.Id == cartItem.Id);
        }

        // Replaces the quantity of a cart item only when
        // it belongs to the specified customer.
        public async Task<CartItem?>
            UpdateCartItemQuantityAsync(
                int cartItemId,
                string userId,
                int quantity)
        {
            // Finds the cart item while checking cart ownership.
            var cartItem = await _context.CartItems
                .Include(item => item.Cart)
                .Include(item => item.FoodItem)
                .FirstOrDefaultAsync(item =>
                    item.Id == cartItemId &&
                    item.Cart != null &&
                    item.Cart.UserId == userId);

            // Returns null when the item does not exist
            // or belongs to another customer.
            if (cartItem == null)
            {
                return null;
            }

            // Replaces the existing quantity.
            cartItem.Quantity = quantity;

            // Saves the updated quantity.
            await _context.SaveChangesAsync();

            // Returns the updated cart item.
            return cartItem;
        }

        // Removes one cart item only when it belongs
        // to the specified customer.
        public async Task<bool> RemoveCartItemAsync(
            int cartItemId,
            string userId)
        {
            // Finds the cart item while checking cart ownership.
            var cartItem = await _context.CartItems
                .FirstOrDefaultAsync(item =>
                    item.Id == cartItemId &&
                    item.Cart != null &&
                    item.Cart.UserId == userId);

            // Returns false when the item does not exist
            // or belongs to another customer.
            if (cartItem == null)
            {
                return false;
            }

            // Marks the cart item for deletion.
            _context.CartItems.Remove(cartItem);

            // Sends the DELETE command to SQL Server.
            await _context.SaveChangesAsync();

            // Confirms that the cart item was removed.
            return true;
        }

        // Removes every item from the specified customer's cart.
        public async Task<bool> ClearCartAsync(
            string userId)
        {
            // Loads the customer's cart and all its items.
            var cart = await _context.Carts
                .Include(existingCart =>
                    existingCart.CartItems)
                .FirstOrDefaultAsync(existingCart =>
                    existingCart.UserId == userId);

            // Returns false when the customer has no cart.
            if (cart == null)
            {
                return false;
            }

            // Removes all CartItem records while keeping
            // the customer's Cart record available for reuse.
            _context.CartItems.RemoveRange(
                cart.CartItems);

            // Saves the deletions to the database.
            await _context.SaveChangesAsync();

            // An already-empty cart is also considered
            // successfully cleared.
            return true;
        }
    }
}