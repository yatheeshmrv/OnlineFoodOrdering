using FluentValidation;
using FoodOrderAPI.DTOs;
using FoodOrderAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FoodOrderAPI.Controllers
{
    // Sets the controller route as api/Order.
    [Route("api/[controller]")]

    // Enables automatic API-controller behaviour.
    [ApiController]
    public class OrderController : ControllerBase
    {
        // Service used for order-related business logic.
        private readonly IOrderService _orderService;

        // FluentValidation validator for CreateOrderDto.
        private readonly IValidator<CreateOrderDto>
            _createOrderValidator;

        // FluentValidation validator for UpdateOrderStatusDto.
        private readonly IValidator<UpdateOrderStatusDto>
            _updateOrderStatusValidator;

        // Constructor injection provides the order service
        // and both order validators.
        public OrderController(
            IOrderService orderService,
            IValidator<CreateOrderDto> createOrderValidator,
            IValidator<UpdateOrderStatusDto>
                updateOrderStatusValidator)
        {
            // Stores the injected order service.
            _orderService = orderService;

            // Stores the create-order validator.
            _createOrderValidator = createOrderValidator;

            // Stores the update-status validator.
            _updateOrderStatusValidator =
                updateOrderStatusValidator;
        }

        // ---------------------------------------------------------
        // GET ALL ORDERS
        // ---------------------------------------------------------

        // Only users with the Admin role can access this endpoint.
        [Authorize(Roles = "Admin")]

        // Handles GET api/Order.
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderDto>>>
            GetAllOrders()
        {
            // Retrieves every order through the service layer.
            var orders =
                await _orderService.GetAllOrdersAsync();

            // Returns HTTP 200 with the orders.
            return Ok(orders);
        }

        // ---------------------------------------------------------
        // GET THE LOGGED-IN CUSTOMER'S ORDERS
        // ---------------------------------------------------------

        // Only users with the Customer role can access this endpoint.
        [Authorize(Roles = "Customer")]

        // Handles GET api/Order/my-orders.
        [HttpGet("my-orders")]
        public async Task<ActionResult<IEnumerable<OrderDto>>>
            GetMyOrders()
        {
            // Reads the logged-in user's Identity ID from the JWT.
            var userId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Rejects the request if the JWT does not contain a user ID.
            if (string.IsNullOrWhiteSpace(userId))
            {
                // Returns HTTP 401 Unauthorized.
                return Unauthorized(new
                {
                    message = "User ID was not found in the token."
                });
            }

            // Retrieves only orders belonging to the logged-in user.
            var orders =
                await _orderService.GetMyOrdersAsync(userId);

            // Returns HTTP 200 with the customer's orders.
            return Ok(orders);
        }

        // ---------------------------------------------------------
        // GET ONE ORDER BELONGING TO THE LOGGED-IN CUSTOMER
        // ---------------------------------------------------------

        // Only users with the Customer role can access this endpoint.
        [Authorize(Roles = "Customer")]

        // Handles GET api/Order/my-orders/{id}.
        [HttpGet("my-orders/{id:int}")]
        public async Task<ActionResult<OrderDto>>
            GetMyOrderById(int id)
        {
            // Reads the logged-in user's Identity ID from the JWT.
            var userId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Rejects the request if the JWT does not contain a user ID.
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized(new
                {
                    message = "User ID was not found in the token."
                });
            }

            // Retrieves the order only when both its ID and owner match.
            var order =
                await _orderService.GetMyOrderByIdAsync(
                    id,
                    userId);

            // Returns the same response when the order does not exist
            // or belongs to a different customer.
            if (order == null)
            {
                return NotFound(new
                {
                    message = "Order not found."
                });
            }

            // Returns HTTP 200 with the customer's order.
            return Ok(order);
        }

        // ---------------------------------------------------------
        // GET AN ORDER BY ID
        // ---------------------------------------------------------

        // Only users with the Admin role can access this endpoint.
        [Authorize(Roles = "Admin")]

        // Handles GET api/Order/{id}.
        [HttpGet("{id:int}")]
        public async Task<ActionResult<OrderDto>>
            GetOrderById(int id)
        {
            // Retrieves the requested order through the service layer.
            var order =
                await _orderService.GetOrderByIdAsync(id);

            // Checks whether the requested order exists.
            if (order == null)
            {
                // Returns HTTP 404 when the order is not found.
                return NotFound(new
                {
                    message = "Order not found."
                });
            }

            // Returns HTTP 200 with the requested order.
            return Ok(order);
        }

        // ---------------------------------------------------------
        // CREATE AN ORDER
        // ---------------------------------------------------------

        // Only users with the Customer role can create orders.
        [Authorize(Roles = "Customer")]

        // Handles POST api/Order.
        [HttpPost]
        public async Task<ActionResult<CreateOrderResponseDto>>
            CreateOrder(
                [FromBody]
                CreateOrderDto createOrderDto)
        {
            // Reads the logged-in customer's ID from the JWT.
            var userId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Ensures the token contains the customer's user ID.
            if (string.IsNullOrWhiteSpace(userId))
            {
                // Returns HTTP 401 when the user ID claim is missing.
                return Unauthorized(new
                {
                    message = "User ID was not found in the token."
                });
            }

            // Executes the rules defined in
            // CreateOrderDtoValidator.
            var validationResult =
                await _createOrderValidator.ValidateAsync(
                    createOrderDto);

            // Checks whether any FluentValidation rules failed.
            if (!validationResult.IsValid)
            {
                // Adds every FluentValidation error to ModelState.
                foreach (var error in validationResult.Errors)
                {
                    ModelState.AddModelError(
                        error.PropertyName,
                        error.ErrorMessage);
                }

                // Returns HTTP 400 with structured validation errors.
                return ValidationProblem(ModelState);
            }

            // Sends the valid request and logged-in user's ID
            // to the service layer.
            var createdOrder =
                await _orderService.CreateOrderAsync(
                    createOrderDto,
                    userId);

            // Returns HTTP 400 when the order could not be created.
            if (!createdOrder.IsSuccess)
            {
                return BadRequest(createdOrder);
            }

            // Returns HTTP 201 with the created-order response.
            return StatusCode(
                StatusCodes.Status201Created,
                createdOrder);
        }

        // ---------------------------------------------------------
        // UPDATE ORDER STATUS
        // ---------------------------------------------------------

        // Only users with the Admin role can update order status.
        [Authorize(Roles = "Admin")]

        // Handles PUT api/Order/{id}/status.
        [HttpPut("{id:int}/status")]
        public async Task<ActionResult<OrderDto>>
            UpdateOrderStatus(
                int id,
                [FromBody]
                UpdateOrderStatusDto statusDto)
        {
            // Executes the rules defined in
            // UpdateOrderStatusDtoValidator.
            var validationResult =
                await _updateOrderStatusValidator
                    .ValidateAsync(statusDto);

            // Checks whether any FluentValidation rules failed.
            if (!validationResult.IsValid)
            {
                // Adds every validation error to ModelState.
                foreach (var error in validationResult.Errors)
                {
                    ModelState.AddModelError(
                        error.PropertyName,
                        error.ErrorMessage);
                }

                // Returns HTTP 400 with structured validation errors.
                return ValidationProblem(ModelState);
            }

            // Sends the validated status and order ID
            // to the service layer.
            var order =
                await _orderService.UpdateOrderStatusAsync(
                    id,
                    statusDto);

            // Checks whether the requested order exists.
            if (order == null)
            {
                // Returns HTTP 404 when the order is not found.
                return NotFound(new
                {
                    message = "Order not found."
                });
            }

            // Returns HTTP 200 with the updated order.
            return Ok(order);
        }

        // ---------------------------------------------------------
        // DELETE AN ORDER
        // ---------------------------------------------------------

        // Only users with the Admin role can delete orders.
        [Authorize(Roles = "Admin")]

        // Handles DELETE api/Order/{id}.
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            // Attempts to delete the order through the service layer.
            var deleted =
                await _orderService.DeleteOrderAsync(id);

            // A false result means the order was not found.
            if (!deleted)
            {
                // Returns HTTP 404 when the order does not exist.
                return NotFound(new
                {
                    message = "Order not found."
                });
            }

            // Returns HTTP 204 after successful deletion.
            return NoContent();
        }
    }
}