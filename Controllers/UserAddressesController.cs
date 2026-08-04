using FluentValidation;
using FoodOrderAPI.DTOs;
using FoodOrderAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FoodOrderAPI.Controllers
{
    // Sets the controller route as api/UserAddresses.
    [Route("api/[controller]")]

    // Enables automatic API-controller behaviour.
    [ApiController]

    // Every endpoint in this controller is available
    // only to users with the Customer role.
    [Authorize(Roles = "Customer")]
    public class UserAddressesController : ControllerBase
    {
        // Service used for saved delivery-address business logic.
        private readonly IUserAddressService
            _userAddressService;

        // FluentValidation validator for CreateUserAddressDto.
        private readonly IValidator<CreateUserAddressDto>
            _createUserAddressValidator;

        // FluentValidation validator for UpdateUserAddressDto.
        private readonly IValidator<UpdateUserAddressDto>
            _updateUserAddressValidator;

        // Constructor injection provides the address service
        // and both request validators.
        public UserAddressesController(
            IUserAddressService userAddressService,
            IValidator<CreateUserAddressDto>
                createUserAddressValidator,
            IValidator<UpdateUserAddressDto>
                updateUserAddressValidator)
        {
            _userAddressService = userAddressService;

            _createUserAddressValidator =
                createUserAddressValidator;

            _updateUserAddressValidator =
                updateUserAddressValidator;
        }

        // ---------------------------------------------------------
        // GET ALL SAVED ADDRESSES
        // ---------------------------------------------------------

        // Handles GET api/UserAddresses.
        [HttpGet]
        public async Task<
            ActionResult<List<UserAddressDto>>>
            GetUserAddresses()
        {
            // Reads the logged-in customer's Identity ID
            // from the JWT.
            var userId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            // Rejects a token without a user ID.
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized(new
                {
                    message =
                        "User ID was not found in the token."
                });
            }

            // Retrieves all addresses belonging
            // to the logged-in customer.
            var addresses =
                await _userAddressService
                    .GetUserAddressesAsync(userId);

            // Returns HTTP 200.
            // An empty list is returned when no addresses exist.
            return Ok(addresses);
        }

        // ---------------------------------------------------------
        // GET ONE SAVED ADDRESS
        // ---------------------------------------------------------

        // Handles GET api/UserAddresses/{addressId}.
        [HttpGet("{addressId:int}")]
        public async Task<ActionResult<UserAddressDto>>
            GetUserAddress(
                int addressId)
        {
            // Reads the logged-in customer's Identity ID
            // from the JWT.
            var userId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            // Rejects a token without a user ID.
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized(new
                {
                    message =
                        "User ID was not found in the token."
                });
            }

            // Retrieves the address while checking ownership.
            var address =
                await _userAddressService
                    .GetUserAddressByIdAsync(
                        addressId,
                        userId);

            // Uses the same response when the address is missing
            // or belongs to another customer.
            if (address == null)
            {
                return NotFound(new
                {
                    message = "Delivery address not found."
                });
            }

            // Returns HTTP 200 with the saved address.
            return Ok(address);
        }

        // ---------------------------------------------------------
        // CREATE SAVED ADDRESS
        // ---------------------------------------------------------

        // Handles POST api/UserAddresses.
        [HttpPost]
        public async Task<ActionResult<UserAddressDto>>
            CreateUserAddress(
                [FromBody]
                CreateUserAddressDto createUserAddressDto)
        {
            // Reads the logged-in customer's Identity ID
            // from the JWT.
            var userId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            // Rejects a token without a user ID.
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized(new
                {
                    message =
                        "User ID was not found in the token."
                });
            }

            // Executes CreateUserAddressDtoValidator.
            var validationResult =
                await _createUserAddressValidator
                    .ValidateAsync(
                        createUserAddressDto);

            // Checks whether validation rules failed.
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

                // Returns HTTP 400 with validation errors.
                return ValidationProblem(ModelState);
            }

            // Creates the address for the authenticated customer.
            var createdAddress =
                await _userAddressService
                    .CreateUserAddressAsync(
                        createUserAddressDto,
                        userId);

            // Returns HTTP 201 with the created address
            // and its GET endpoint location.
            return CreatedAtAction(
                nameof(GetUserAddress),
                new
                {
                    addressId = createdAddress.Id
                },
                createdAddress);
        }

        // ---------------------------------------------------------
        // UPDATE SAVED ADDRESS
        // ---------------------------------------------------------

        // Handles PUT api/UserAddresses/{addressId}.
        [HttpPut("{addressId:int}")]
        public async Task<ActionResult<UserAddressDto>>
            UpdateUserAddress(
                int addressId,
                [FromBody]
                UpdateUserAddressDto updateUserAddressDto)
        {
            // Reads the logged-in customer's Identity ID
            // from the JWT.
            var userId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            // Rejects a token without a user ID.
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized(new
                {
                    message =
                        "User ID was not found in the token."
                });
            }

            // Executes UpdateUserAddressDtoValidator.
            var validationResult =
                await _updateUserAddressValidator
                    .ValidateAsync(
                        updateUserAddressDto);

            // Checks whether validation rules failed.
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

                // Returns HTTP 400 with validation errors.
                return ValidationProblem(ModelState);
            }

            // Updates the address while checking ownership.
            var updatedAddress =
                await _userAddressService
                    .UpdateUserAddressAsync(
                        addressId,
                        updateUserAddressDto,
                        userId);

            // Uses the same response when the address is missing
            // or belongs to another customer.
            if (updatedAddress == null)
            {
                return NotFound(new
                {
                    message = "Delivery address not found."
                });
            }

            // Returns HTTP 200 with the updated address.
            return Ok(updatedAddress);
        }

        // ---------------------------------------------------------
        // DELETE SAVED ADDRESS
        // ---------------------------------------------------------

        // Handles DELETE api/UserAddresses/{addressId}.
        [HttpDelete("{addressId:int}")]
        public async Task<IActionResult>
            DeleteUserAddress(
                int addressId)
        {
            // Reads the logged-in customer's Identity ID
            // from the JWT.
            var userId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            // Rejects a token without a user ID.
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized(new
                {
                    message =
                        "User ID was not found in the token."
                });
            }

            // Deletes the address while checking ownership.
            var deleted =
                await _userAddressService
                    .DeleteUserAddressAsync(
                        addressId,
                        userId);

            // Uses the same response when the address is missing
            // or belongs to another customer.
            if (!deleted)
            {
                return NotFound(new
                {
                    message = "Delivery address not found."
                });
            }

            // Returns HTTP 204 after successful deletion.
            return NoContent();
        }

        // ---------------------------------------------------------
        // SET DEFAULT ADDRESS
        // ---------------------------------------------------------

        // Handles PUT api/UserAddresses/{addressId}/default.
        [HttpPut("{addressId:int}/default")]
        public async Task<ActionResult<UserAddressDto>>
            SetDefaultAddress(
                int addressId)
        {
            // Reads the logged-in customer's Identity ID
            // from the JWT.
            var userId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            // Rejects a token without a user ID.
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized(new
                {
                    message =
                        "User ID was not found in the token."
                });
            }

            // Sets the selected address as default while
            // checking that it belongs to this customer.
            var updatedAddress =
                await _userAddressService
                    .SetDefaultAddressAsync(
                        addressId,
                        userId);

            // Uses the same response when the address is missing
            // or belongs to another customer.
            if (updatedAddress == null)
            {
                return NotFound(new
                {
                    message = "Delivery address not found."
                });
            }

            // Returns HTTP 200 with the new default address.
            return Ok(updatedAddress);
        }
    }
}