using FluentValidation;
using FoodOrderAPI.DTOs;
using FoodOrderAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FoodOrderAPI.Controllers
{
    // Sets the controller route as api/Cart.
    [Route("api/[controller]")]

    // Enables automatic API-controller behaviour.
    [ApiController]

    // Every endpoint in this controller is available
    // only to users with the Customer role.
    [Authorize(Roles = "Customer")]
    public class CartController : ControllerBase
    {
        // Service used for shopping-cart business logic.
        private readonly ICartService _cartService;

        // FluentValidation validator for AddCartItemDto.
        private readonly IValidator<AddCartItemDto>
            _addCartItemValidator;

        // FluentValidation validator for
        // UpdateCartItemQuantityDto.
        private readonly IValidator<UpdateCartItemQuantityDto>
            _updateCartItemQuantityValidator;

        // Constructor injection provides the cart service
        // and both request validators.
        public CartController(
            ICartService cartService,
            IValidator<AddCartItemDto> addCartItemValidator,
            IValidator<UpdateCartItemQuantityDto>
                updateCartItemQuantityValidator)
        {
            // Stores the injected cart service.
            _cartService = cartService;

            // Stores the add-to-cart validator.
            _addCartItemValidator = addCartItemValidator;

            // Stores the quantity-update validator.
            _updateCartItemQuantityValidator =
                updateCartItemQuantityValidator;
        }

        // ---------------------------------------------------------
        // GET THE LOGGED-IN CUSTOMER'S CART
        // ---------------------------------------------------------

        // Handles GET api/Cart.
        [HttpGet]
        public async Task<ActionResult<CartDto>> GetCart()
        {
            // Reads the logged-in customer's Identity ID
            // from the JWT.
            var userId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Rejects a token without a user ID.
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized(new
                {
                    message = "User ID was not found in the token."
                });
            }

            // Retrieves or creates the customer's cart.
            var cart =
                await _cartService.GetCartAsync(userId);

            // Returns HTTP 200 with the cart.
            return Ok(cart);
        }

        // ---------------------------------------------------------
        // ADD A FOOD ITEM TO THE CART
        // ---------------------------------------------------------

        // Handles POST api/Cart/items.
        [HttpPost("items")]
        public async Task<ActionResult<CartDto>>
            AddCartItem(
                [FromBody]
                AddCartItemDto addCartItemDto)
        {
            // Reads the logged-in customer's Identity ID
            // from the JWT.
            var userId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Rejects a token without a user ID.
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized(new
                {
                    message = "User ID was not found in the token."
                });
            }

            // Executes AddCartItemDtoValidator.
            var validationResult =
                await _addCartItemValidator.ValidateAsync(
                    addCartItemDto);

            // Checks whether any validation rules failed.
            if (!validationResult.IsValid)
            {
                // Adds every validation error to ModelState.
                foreach (var error in validationResult.Errors)
                {
                    ModelState.AddModelError(
                        error.PropertyName,
                        error.ErrorMessage);
                }

                // Returns HTTP 400 with validation errors.
                return ValidationProblem(ModelState);
            }

            // Adds the item or increases its existing quantity.
            var updatedCart =
                await _cartService.AddCartItemAsync(
                    addCartItemDto,
                    userId);

            // Returns HTTP 200 with the updated cart.
            return Ok(updatedCart);
        }

        // ---------------------------------------------------------
        // UPDATE A CART ITEM'S QUANTITY
        // ---------------------------------------------------------

        // Handles PUT api/Cart/items/{cartItemId}.
        [HttpPut("items/{cartItemId:int}")]
        public async Task<ActionResult<CartDto>>
            UpdateCartItemQuantity(
                int cartItemId,
                [FromBody]
                UpdateCartItemQuantityDto quantityDto)
        {
            // Reads the logged-in customer's Identity ID
            // from the JWT.
            var userId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Rejects a token without a user ID.
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized(new
                {
                    message = "User ID was not found in the token."
                });
            }

            // Executes UpdateCartItemQuantityDtoValidator.
            var validationResult =
                await _updateCartItemQuantityValidator
                    .ValidateAsync(quantityDto);

            // Checks whether any validation rules failed.
            if (!validationResult.IsValid)
            {
                // Adds every validation error to ModelState.
                foreach (var error in validationResult.Errors)
                {
                    ModelState.AddModelError(
                        error.PropertyName,
                        error.ErrorMessage);
                }

                // Returns HTTP 400 with validation errors.
                return ValidationProblem(ModelState);
            }

            // Updates the quantity only when the item
            // belongs to the logged-in customer.
            var updatedCart =
                await _cartService
                    .UpdateCartItemQuantityAsync(
                        cartItemId,
                        quantityDto,
                        userId);

            // Returns the same response when the item is missing
            // or belongs to another customer.
            if (updatedCart == null)
            {
                return NotFound(new
                {
                    message = "Cart item not found."
                });
            }

            // Returns HTTP 200 with the updated cart.
            return Ok(updatedCart);
        }

        // ---------------------------------------------------------
        // REMOVE A CART ITEM
        // ---------------------------------------------------------

        // Handles DELETE api/Cart/items/{cartItemId}.
        [HttpDelete("items/{cartItemId:int}")]
        public async Task<IActionResult> RemoveCartItem(
            int cartItemId)
        {
            // Reads the logged-in customer's Identity ID
            // from the JWT.
            var userId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Rejects a token without a user ID.
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized(new
                {
                    message = "User ID was not found in the token."
                });
            }

            // Removes the item only when it belongs
            // to the logged-in customer's cart.
            var removed =
                await _cartService.RemoveCartItemAsync(
                    cartItemId,
                    userId);

            // Returns the same response when the item is missing
            // or belongs to another customer.
            if (!removed)
            {
                return NotFound(new
                {
                    message = "Cart item not found."
                });
            }

            // Returns HTTP 204 after successful removal.
            return NoContent();
        }

        // ---------------------------------------------------------
        // CHECKOUT
        // ---------------------------------------------------------

        // Handles POST api/Cart/checkout.
        // No request body is required because checkout uses
        // the logged-in customer's existing cart.
        [HttpPost("checkout")]
        public async Task<ActionResult<CreateOrderResponseDto>>
            Checkout()
        {
            // Reads the logged-in customer's Identity ID
            // from the JWT.
            var userId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Rejects a token without a user ID.
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized(new
                {
                    message = "User ID was not found in the token."
                });
            }

            // Attempts to convert the customer's cart into an order.
            var checkoutResult =
                await _cartService.CheckoutAsync(userId);

            // Returns HTTP 400 when checkout is rejected,
            // such as when the cart is empty or an item
            // is no longer available.
            if (!checkoutResult.IsSuccess)
            {
                return BadRequest(checkoutResult);
            }

            // Returns HTTP 200 with the created order.
            return Ok(checkoutResult);
        }

        // ---------------------------------------------------------
        // CLEAR THE CART
        // ---------------------------------------------------------

        // Handles DELETE api/Cart.
        [HttpDelete]
        public async Task<IActionResult> ClearCart()
        {
            // Reads the logged-in customer's Identity ID
            // from the JWT.
            var userId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Rejects a token without a user ID.
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized(new
                {
                    message = "User ID was not found in the token."
                });
            }

            // Removes all items while keeping the Cart record.
            var cleared =
                await _cartService.ClearCartAsync(userId);

            // Handles an unexpected failure to locate
            // or clear the customer's cart.
            if (!cleared)
            {
                return NotFound(new
                {
                    message = "Cart not found."
                });
            }

            // Returns HTTP 204 after successful clearing.
            return NoContent();
        }
    }
}