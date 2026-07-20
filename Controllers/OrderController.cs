using FoodOrderAPI.Models;
using FoodOrderAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace FoodOrderAPI.Controllers
{
    // This tells ASP.NET Core that this class is an API controller.
    [ApiController]

    // This sets the API URL.
    // Since the controller name is OrderController,
    // the URL will be: api/order
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        // This field stores the order service object.
        // Controller uses service to perform order-related operations.
        private readonly IOrderService _orderService;

        // Constructor injection.
        // ASP.NET Core gives the OrderService object automatically using Dependency Injection.
        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        // GET: api/order
        // This method returns all orders.
        [HttpGet]
        public async Task<ActionResult<List<Order>>> GetAllOrders()
        {
            // Calls service method to get all orders.
            List<Order> orders = await _orderService.GetAllOrdersAsync();

            // Returns 200 OK with order list.
            return Ok(orders);
        }

        // GET: api/order/1
        // This method returns one order based on id.
        [HttpGet("{id}")]
        public async Task<ActionResult<Order>> GetOrderById(int id)
        {
            // Calls service method to find order by id.
            Order? order = await _orderService.GetOrderByIdAsync(id);

            // If order is not found, return 404 Not Found.
            if (order == null)
            {
                return NotFound("Order not found");
            }

            // If order is found, return 200 OK with order details.
            return Ok(order);
        }

        // POST: api/order
        // This method creates a new order.
        [HttpPost]
        public async Task<ActionResult<Order>> CreateOrder(Order order)
        {
            // Sends the order object to service for creation.
            Order createdOrder = await _orderService.CreateOrderAsync(order);

            // Returns 201 Created response with created order data.
            return CreatedAtAction(
                nameof(GetOrderById),
                new { id = createdOrder.Id },
                createdOrder
            );
        }

        // PUT: api/order/1/status
        // This method updates only the order status.
        [HttpPut("{id}/status")]
        public async Task<ActionResult<Order>> UpdateOrderStatus(int id, string orderStatus)
        {
            // Sends id and new status to service.
            Order? updatedOrder = await _orderService.UpdateOrderStatusAsync(id, orderStatus);

            // If order is not found, return 404 Not Found.
            if (updatedOrder == null)
            {
                return NotFound("Order not found");
            }

            // If order is updated, return 200 OK with updated order.
            return Ok(updatedOrder);
        }

        // DELETE: api/order/1
        // This method deletes an order by id.
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteOrder(int id)
        {
            // Calls service to delete order.
            bool isDeleted = await _orderService.DeleteOrderAsync(id);

            // If order is not found, return 404 Not Found.
            if (isDeleted == false)
            {
                return NotFound("Order not found");
            }

            // If deleted successfully, return 204 No Content.
            return NoContent();
        }
    }
}