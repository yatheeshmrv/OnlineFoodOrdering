using Microsoft.AspNetCore.Mvc;
using FoodOrderAPI.DTOs;
using FoodOrderAPI.Services;

namespace FoodOrderAPI.Controllers
{
    // [ApiController] enables automatic model validation.
    // Example: If required fields are missing, ASP.NET Core returns 400 Bad Request automatically.
    [ApiController]

    // Route becomes: api/Orders
    // Because controller name is OrdersController.
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        // This is the service object.
        // Controller does not directly talk to repository.
        // Controller talks to service.
        private readonly IOrderService _orderService;

        // Constructor Injection:
        // ASP.NET Core injects OrderService here through IOrderService.
        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        // GET: api/Orders
        // This API gets all orders.
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetAllOrders()
        {
            // Ask service to get all orders.
            var orders = await _orderService.GetAllOrdersAsync();

            // Return 200 OK with all orders.
            return Ok(orders);
        }

        // GET: api/Orders/1
        // This API gets one order by id.
        [HttpGet("{id}")]
        public async Task<ActionResult<OrderDto>> GetOrderById(int id)
        {
            // Ask service to find order by id.
            var order = await _orderService.GetOrderByIdAsync(id);

            // If order is not found, return 404 Not Found.
            if (order == null)
            {
                return NotFound("Order not found");
            }

            // If order exists, return 200 OK with order details.
            return Ok(order);
        }

        // POST: api/Orders
        // This API is used to create a new order.
        [HttpPost]
        public async Task<ActionResult<CreateOrderResponseDto>> CreateOrder(CreateOrderDto createOrderDto)
        {
            // Call OrderService to create the order.
            // Now service returns CreateOrderResponseDto.
            // This response contains:
            // 1. IsSuccess
            // 2. Message
            // 3. Order details
            var response = await _orderService.CreateOrderAsync(createOrderDto);

            // If IsSuccess is false, order was not created.
            // Possible reasons:
            // 1. Food item not found
            // 2. Food item is currently unavailable
            if (response.IsSuccess == false)
            {
                return BadRequest(response);
            }

            // If order is created successfully, response.Order will contain order details.
            // We use response.Order!.Id because Order will not be null when IsSuccess is true.
            return CreatedAtAction(
                nameof(GetOrderById),
                new { id = response.Order!.Id },
                response
            );
        }


        // PUT: api/Orders/1/status
        // This API updates only order status.
        [HttpPut("{id}/status")]
        public async Task<ActionResult<OrderDto>> UpdateOrderStatus(
            int id,
            UpdateOrderStatusDto updateOrderStatusDto)
        {
            // Ask service to update order status.
            var updatedOrder = await _orderService.UpdateOrderStatusAsync(id, updateOrderStatusDto);

            // If order is not found, return 404 Not Found.
            if (updatedOrder == null)
            {
                return NotFound("Order not found");
            }

            // Return 200 OK with updated order.
            return Ok(updatedOrder);
        }

        // DELETE: api/Orders/1
        // This API deletes one order by id.
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            // Ask service to delete order.
            var result = await _orderService.DeleteOrderAsync(id);

            // If result is false, order was not found.
            if (result == false)
            {
                return NotFound("Order not found");
            }

            // If deleted successfully, return 200 OK.
            return Ok("Order deleted successfully");
        }
    }
}
