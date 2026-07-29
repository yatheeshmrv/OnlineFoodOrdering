using FoodOrderAPI.Data;
using FoodOrderAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace FoodOrderAPI.Repositories
{
    // Handles database operations related to orders.
    public class OrderRepository : IOrderRepository
    {
        // EF Core database context.
        private readonly ApplicationDbContext _context;

        // Receives the database context through dependency injection.
        public OrderRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // Returns all orders for Admin.
        public async Task<IEnumerable<Order>> GetAllOrdersAsync()
        {
            return await _context.Orders
                .Include(order => order.OrderItems)
                .ThenInclude(orderItem => orderItem.FoodItem)
                .AsNoTracking()
                .OrderByDescending(order => order.OrderDate)
                .ToListAsync();
        }

        // Returns one order using its order ID.
        public async Task<Order?> GetOrderByIdAsync(int id)
        {
            return await _context.Orders
                .Include(order => order.OrderItems)
                .ThenInclude(orderItem => orderItem.FoodItem)
                .AsNoTracking()
                .FirstOrDefaultAsync(order => order.Id == id);
        }

        // Returns an order only when it belongs to the specified customer.
        public async Task<Order?> GetOrderByIdAndUserIdAsync(
            int id,
            string userId)
        {
            return await _context.Orders
                .Include(order => order.OrderItems)
                .ThenInclude(orderItem => orderItem.FoodItem)
                .AsNoTracking()
                .FirstOrDefaultAsync(order =>
                    order.Id == id &&
                    order.UserId == userId);
        }

        // Returns only the orders placed by the logged-in customer.
        public async Task<IEnumerable<Order>> GetOrdersByUserIdAsync(
            string userId)
        {
            return await _context.Orders
                .Where(order => order.UserId == userId)
                .Include(order => order.OrderItems)
                .ThenInclude(orderItem => orderItem.FoodItem)
                .AsNoTracking()
                .OrderByDescending(order => order.OrderDate)
                .ToListAsync();
        }

        // Creates a new order.
        public async Task<Order> CreateOrderAsync(Order order)
        {
            foreach (var orderItem in order.OrderItems)
            {
                // FoodItemId already identifies the existing food item.
                // Prevents EF Core from trying to insert it again.
                orderItem.FoodItem = null;
            }

            // Marks the new order and its order items for insertion.
            await _context.Orders.AddAsync(order);

            // Saves the new order to the database.
            await _context.SaveChangesAsync();

            return order;
        }

        // Creates an order from the cart and clears its cart items.
        public async Task<Order> CreateOrderFromCartAsync(
            Order order,
            int cartId)
        {
            foreach (var orderItem in order.OrderItems)
            {
                // FoodItemId already references an existing food item.
                // Prevents EF Core from trying to insert it again.
                orderItem.FoodItem = null;
            }

            // Loads the items belonging to the cart being checked out.
            var cartItems = await _context.CartItems
                .Where(cartItem => cartItem.CartId == cartId)
                .ToListAsync();

            // Marks the new order and its order items for insertion.
            await _context.Orders.AddAsync(order);

            // Marks all cart items for deletion while retaining the cart.
            _context.CartItems.RemoveRange(cartItems);

            // Inserts the order and deletes the cart items together.
            // EF Core executes this SaveChanges operation transactionally.
            await _context.SaveChangesAsync();

            return order;
        }

        // Updates the status of an existing order.
        public async Task<Order?> UpdateOrderStatusAsync(
            int id,
            string orderStatus)
        {
            var order = await _context.Orders
                .Include(order => order.OrderItems)
                .ThenInclude(orderItem => orderItem.FoodItem)
                .FirstOrDefaultAsync(order => order.Id == id);

            if (order == null)
            {
                return null;
            }

            // Applies the new status to the order.
            order.OrderStatus = orderStatus;

            // Saves the status change.
            await _context.SaveChangesAsync();

            return order;
        }

        // Deletes an order using its ID.
        public async Task<bool> DeleteOrderAsync(int id)
        {
            var order = await _context.Orders.FindAsync(id);

            if (order == null)
            {
                return false;
            }

            // Marks the order for deletion.
            _context.Orders.Remove(order);

            // Saves the deletion.
            await _context.SaveChangesAsync();

            return true;
        }
    }
}