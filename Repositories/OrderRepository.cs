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

        public async Task<IEnumerable<Order>> GetAllOrdersAsync()
        {
            return await _context.Orders
                .Include(order => order.OrderItems)
                .ThenInclude(orderItem => orderItem.FoodItem)
                .AsNoTracking()
                .OrderByDescending(order => order.OrderDate)
                .ToListAsync();
        }

        public async Task<Order?> GetOrderByIdAsync(int id)
        {
            return await _context.Orders
                .Include(order => order.OrderItems)
                .ThenInclude(orderItem => orderItem.FoodItem)
                .AsNoTracking()
                .FirstOrDefaultAsync(order => order.Id == id);
        }

        public async Task<Order> CreateOrderAsync(Order order)
        {
            foreach (var orderItem in order.OrderItems)
            {
                // FoodItemId is sufficient. The existing FoodItem entity
                // should not be inserted again.
                orderItem.FoodItem = null;
            }

            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();

            return order;
        }

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