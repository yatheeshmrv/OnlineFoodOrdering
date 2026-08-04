using FoodOrderAPI.DTOs;

namespace FoodOrderAPI.Services
{
    // Defines the business operations available for managing
    // delivery addresses belonging to a registered customer.
    public interface IUserAddressService
    {
        // Returns all saved delivery addresses belonging
        // to the logged-in customer.
        Task<List<UserAddressDto>> GetUserAddressesAsync(
            string userId);

        // Returns one saved address only when it belongs
        // to the logged-in customer.
        //
        // Returns null when the address does not exist
        // or belongs to another customer.
        Task<UserAddressDto?> GetUserAddressByIdAsync(
            int addressId,
            string userId);

        // Creates a new saved delivery address for
        // the logged-in customer.
        //
        // The first saved address automatically becomes default.
        Task<UserAddressDto> CreateUserAddressAsync(
            CreateUserAddressDto createUserAddressDto,
            string userId);

        // Updates an existing saved delivery address only when
        // it belongs to the logged-in customer.
        //
        // Returns null when the address does not exist
        // or belongs to another customer.
        Task<UserAddressDto?> UpdateUserAddressAsync(
            int addressId,
            UpdateUserAddressDto updateUserAddressDto,
            string userId);

        // Deletes a saved delivery address only when it belongs
        // to the logged-in customer.
        //
        // Returns false when the address does not exist
        // or belongs to another customer.
        Task<bool> DeleteUserAddressAsync(
            int addressId,
            string userId);

        // Marks one saved address as the customer's default address.
        //
        // Returns null when the address does not exist
        // or belongs to another customer.
        Task<UserAddressDto?> SetDefaultAddressAsync(
            int addressId,
            string userId);
    }
}