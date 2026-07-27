using FoodOrderAPI.Data;
using FoodOrderAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace FoodOrderAPI.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly ApplicationDbContext _context;

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
                // Prevent EF Core from trying to insert it again.
                orderItem.FoodItem = null;
            }

            await _context.Orders.AddAsync(order);
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

            order.OrderStatus = orderStatus;

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

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}