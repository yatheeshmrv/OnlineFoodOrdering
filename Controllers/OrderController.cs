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

        // FluentValidation validator for UpdateOrderStatusDto.
        private readonly IValidator<UpdateOrderStatusDto>
            _updateOrderStatusValidator;

        // Constructor injection provides the order service
        // and order-status validator.
        public OrderController(
            IOrderService orderService,
            IValidator<UpdateOrderStatusDto>
                updateOrderStatusValidator)
        {
            // Stores the injected order service.
            _orderService = orderService;

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
            // Reads the logged-in user's Identity ID
            // from the JWT.
            var userId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            // Rejects the request when the JWT
            // does not contain a user ID.
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized(new
                {
                    message =
                        "User ID was not found in the token."
                });
            }

            // Retrieves only orders belonging
            // to the logged-in customer.
            var orders =
                await _orderService.GetMyOrdersAsync(
                    userId);

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
            GetMyOrderById(
                int id)
        {
            // Reads the logged-in user's Identity ID
            // from the JWT.
            var userId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            // Rejects the request when the JWT
            // does not contain a user ID.
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized(new
                {
                    message =
                        "User ID was not found in the token."
                });
            }

            // Retrieves the order only when both
            // its ID and owner match.
            var order =
                await _orderService
                    .GetMyOrderByIdAsync(
                        id,
                        userId);

            // Returns the same response when the order
            // does not exist or belongs to another customer.
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
            GetOrderById(
                int id)
        {
            // Retrieves the requested order
            // through the service layer.
            var order =
                await _orderService.GetOrderByIdAsync(
                    id);

            // Checks whether the requested order exists.
            if (order == null)
            {
                return NotFound(new
                {
                    message = "Order not found."
                });
            }

            // Returns HTTP 200 with the requested order.
            return Ok(order);
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

            // Checks whether any validation rules failed.
            if (!validationResult.IsValid)
            {
                // Adds every validation error to ModelState.
                foreach (var error in
                    validationResult.Errors)
                {
                    ModelState.AddModelError(
                        error.PropertyName,
                        error.ErrorMessage);
                }

                // Returns HTTP 400 with structured
                // validation errors.
                return ValidationProblem(ModelState);
            }

            // Sends the validated status and order ID
            // to the service layer.
            var order =
                await _orderService
                    .UpdateOrderStatusAsync(
                        id,
                        statusDto);

            // Checks whether the requested order exists.
            if (order == null)
            {
                return NotFound(new
                {
                    message = "Order not found."
                });
            }

            // Returns HTTP 200 with the updated order.
            return Ok(order);
        }
    }
}