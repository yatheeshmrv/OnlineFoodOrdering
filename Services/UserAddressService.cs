using FoodOrderAPI.DTOs;
using FoodOrderAPI.Models;
using FoodOrderAPI.Repositories;

namespace FoodOrderAPI.Services
{
    // Contains the business logic for customer
    // delivery-address operations.
    public class UserAddressService
        : IUserAddressService
    {
        // Provides access to saved-address database operations.
        private readonly IUserAddressRepository
            _userAddressRepository;

        // Receives the required repository through
        // dependency injection.
        public UserAddressService(
            IUserAddressRepository userAddressRepository)
        {
            _userAddressRepository =
                userAddressRepository;
        }

        // ---------------------------------------------------------
        // GET CUSTOMER'S SAVED ADDRESSES
        // ---------------------------------------------------------

        // Returns all delivery addresses belonging
        // to the logged-in customer.
        public async Task<List<UserAddressDto>>
            GetUserAddressesAsync(
                string userId)
        {
            ValidateUserId(userId);

            var addresses =
                await _userAddressRepository
                    .GetUserAddressesAsync(userId);

            return addresses
                .Select(MapUserAddress)
                .ToList();
        }

        // ---------------------------------------------------------
        // GET ONE SAVED ADDRESS
        // ---------------------------------------------------------

        // Returns one saved address only when it belongs
        // to the logged-in customer.
        public async Task<UserAddressDto?>
            GetUserAddressByIdAsync(
                int addressId,
                string userId)
        {
            ValidateUserId(userId);
            ValidateAddressId(addressId);

            var address =
                await _userAddressRepository
                    .GetUserAddressByIdAsync(
                        addressId,
                        userId);

            return address == null
                ? null
                : MapUserAddress(address);
        }

        // ---------------------------------------------------------
        // CREATE SAVED ADDRESS
        // ---------------------------------------------------------

        // Creates a reusable delivery address belonging
        // to the logged-in customer.
        public async Task<UserAddressDto>
            CreateUserAddressAsync(
                CreateUserAddressDto createUserAddressDto,
                string userId)
        {
            ValidateUserId(userId);

            // Prevents a null DTO from reaching
            // the business logic.
            ArgumentNullException.ThrowIfNull(
                createUserAddressDto);

            // Keeps direct service calls safe even when they
            // do not pass through FluentValidation.
            ValidateAddressFields(
                createUserAddressDto.AddressLabel,
                createUserAddressDto.RecipientName,
                createUserAddressDto.RecipientPhone,
                createUserAddressDto.AddressLine1,
                createUserAddressDto.City,
                createUserAddressDto.State,
                createUserAddressDto.PostalCode);

            var userAddress = new UserAddress
            {
                // UserId comes from the authenticated JWT
                // and is never accepted from the client.
                UserId = userId,

                AddressLabel =
                    createUserAddressDto
                        .AddressLabel
                        .Trim(),

                RecipientName =
                    createUserAddressDto
                        .RecipientName
                        .Trim(),

                RecipientPhone =
                    createUserAddressDto
                        .RecipientPhone
                        .Trim(),

                AddressLine1 =
                    createUserAddressDto
                        .AddressLine1
                        .Trim(),

                AddressLine2 =
                    NormalizeOptionalText(
                        createUserAddressDto
                            .AddressLine2),

                Landmark =
                    NormalizeOptionalText(
                        createUserAddressDto
                            .Landmark),

                City =
                    createUserAddressDto
                        .City
                        .Trim(),

                State =
                    createUserAddressDto
                        .State
                        .Trim(),

                PostalCode =
                    createUserAddressDto
                        .PostalCode
                        .Trim(),

                IsDefault =
                    createUserAddressDto.IsDefault
            };

            var createdAddress =
                await _userAddressRepository
                    .AddUserAddressAsync(userAddress);

            return MapUserAddress(createdAddress);
        }

        // ---------------------------------------------------------
        // UPDATE SAVED ADDRESS
        // ---------------------------------------------------------

        // Updates a saved address only when it belongs
        // to the logged-in customer.
        public async Task<UserAddressDto?>
            UpdateUserAddressAsync(
                int addressId,
                UpdateUserAddressDto updateUserAddressDto,
                string userId)
        {
            ValidateUserId(userId);
            ValidateAddressId(addressId);

            // Prevents a null DTO from reaching
            // the business logic.
            ArgumentNullException.ThrowIfNull(
                updateUserAddressDto);

            // Keeps direct service calls safe even when they
            // do not pass through FluentValidation.
            ValidateAddressFields(
                updateUserAddressDto.AddressLabel,
                updateUserAddressDto.RecipientName,
                updateUserAddressDto.RecipientPhone,
                updateUserAddressDto.AddressLine1,
                updateUserAddressDto.City,
                updateUserAddressDto.State,
                updateUserAddressDto.PostalCode);

            var updatedAddress = new UserAddress
            {
                AddressLabel =
                    updateUserAddressDto
                        .AddressLabel
                        .Trim(),

                RecipientName =
                    updateUserAddressDto
                        .RecipientName
                        .Trim(),

                RecipientPhone =
                    updateUserAddressDto
                        .RecipientPhone
                        .Trim(),

                AddressLine1 =
                    updateUserAddressDto
                        .AddressLine1
                        .Trim(),

                AddressLine2 =
                    NormalizeOptionalText(
                        updateUserAddressDto
                            .AddressLine2),

                Landmark =
                    NormalizeOptionalText(
                        updateUserAddressDto
                            .Landmark),

                City =
                    updateUserAddressDto
                        .City
                        .Trim(),

                State =
                    updateUserAddressDto
                        .State
                        .Trim(),

                PostalCode =
                    updateUserAddressDto
                        .PostalCode
                        .Trim(),

                IsDefault =
                    updateUserAddressDto.IsDefault
            };

            var savedAddress =
                await _userAddressRepository
                    .UpdateUserAddressAsync(
                        addressId,
                        userId,
                        updatedAddress);

            return savedAddress == null
                ? null
                : MapUserAddress(savedAddress);
        }

        // ---------------------------------------------------------
        // DELETE SAVED ADDRESS
        // ---------------------------------------------------------

        // Deletes an address only when it belongs
        // to the logged-in customer.
        public async Task<bool>
            DeleteUserAddressAsync(
                int addressId,
                string userId)
        {
            ValidateUserId(userId);
            ValidateAddressId(addressId);

            return await _userAddressRepository
                .DeleteUserAddressAsync(
                    addressId,
                    userId);
        }

        // ---------------------------------------------------------
        // SET DEFAULT ADDRESS
        // ---------------------------------------------------------

        // Marks one saved address as the customer's
        // preferred delivery address.
        public async Task<UserAddressDto?>
            SetDefaultAddressAsync(
                int addressId,
                string userId)
        {
            ValidateUserId(userId);
            ValidateAddressId(addressId);

            var address =
                await _userAddressRepository
                    .SetDefaultAddressAsync(
                        addressId,
                        userId);

            return address == null
                ? null
                : MapUserAddress(address);
        }

        // ---------------------------------------------------------
        // VALIDATE USER ID
        // ---------------------------------------------------------

        // Ensures that the authenticated user's ID is available.
        private static void ValidateUserId(
            string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException(
                    "User ID is required.",
                    nameof(userId));
            }
        }

        // ---------------------------------------------------------
        // VALIDATE ADDRESS ID
        // ---------------------------------------------------------

        // Ensures that a valid saved-address ID was supplied.
        private static void ValidateAddressId(
            int addressId)
        {
            if (addressId <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(addressId),
                    "Address ID must be greater than zero.");
            }
        }

        // ---------------------------------------------------------
        // VALIDATE REQUIRED ADDRESS FIELDS
        // ---------------------------------------------------------

        // Protects direct service calls that do not pass through
        // the API's FluentValidation pipeline.
        private static void ValidateAddressFields(
            string addressLabel,
            string recipientName,
            string recipientPhone,
            string addressLine1,
            string city,
            string state,
            string postalCode)
        {
            if (string.IsNullOrWhiteSpace(addressLabel))
            {
                throw new ArgumentException(
                    "Address label is required.");
            }

            if (string.IsNullOrWhiteSpace(recipientName))
            {
                throw new ArgumentException(
                    "Recipient name is required.");
            }

            if (string.IsNullOrWhiteSpace(recipientPhone))
            {
                throw new ArgumentException(
                    "Recipient phone number is required.");
            }

            if (string.IsNullOrWhiteSpace(addressLine1))
            {
                throw new ArgumentException(
                    "Address line 1 is required.");
            }

            if (string.IsNullOrWhiteSpace(city))
            {
                throw new ArgumentException(
                    "City is required.");
            }

            if (string.IsNullOrWhiteSpace(state))
            {
                throw new ArgumentException(
                    "State is required.");
            }

            if (string.IsNullOrWhiteSpace(postalCode))
            {
                throw new ArgumentException(
                    "Postal code is required.");
            }
        }

        // ---------------------------------------------------------
        // NORMALIZE OPTIONAL TEXT
        // ---------------------------------------------------------

        // Converts empty optional values to null and removes
        // unnecessary surrounding whitespace.
        private static string? NormalizeOptionalText(
            string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }

        // ---------------------------------------------------------
        // MAP USER ADDRESS
        // ---------------------------------------------------------

        // Converts a UserAddress entity into a DTO response.
        private static UserAddressDto MapUserAddress(
            UserAddress address)
        {
            return new UserAddressDto
            {
                Id = address.Id,
                AddressLabel = address.AddressLabel,
                RecipientName = address.RecipientName,
                RecipientPhone = address.RecipientPhone,
                AddressLine1 = address.AddressLine1,
                AddressLine2 = address.AddressLine2,
                Landmark = address.Landmark,
                City = address.City,
                State = address.State,
                PostalCode = address.PostalCode,
                IsDefault = address.IsDefault
            };
        }
    }
}