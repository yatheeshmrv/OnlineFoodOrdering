using FoodOrderAPI.DTOs;
using FoodOrderAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace FoodOrderAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet]
        public async Task<ActionResult<List<OrderDto>>> GetAllOrders()
        {
            return Ok(await _orderService.GetAllOrdersAsync());
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<OrderDto>> GetOrderById(int id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            return order == null ? NotFound("Order not found.") : Ok(order);
        }

        [HttpPost]
        public async Task<ActionResult<CreateOrderResponseDto>> CreateOrder(
            [FromBody] CreateOrderDto createOrderDto)
        {
            var result = await _orderService.CreateOrderAsync(createOrderDto);

            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            return CreatedAtAction(
                nameof(GetOrderById),
                new { id = result.Order!.Id },
                result);
        }

        [HttpPut("{id:int}/status")]
        public async Task<ActionResult<OrderDto>> UpdateOrderStatus(
            int id,
            [FromBody] UpdateOrderStatusDto statusDto)
        {
            var order = await _orderService.UpdateOrderStatusAsync(id, statusDto);
            return order == null ? NotFound("Order not found.") : Ok(order);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            return await _orderService.DeleteOrderAsync(id)
                ? NoContent()
                : NotFound("Order not found.");
        }
    }
}