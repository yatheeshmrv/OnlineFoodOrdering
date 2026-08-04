using FoodOrderAPI.Models;

namespace FoodOrderAPI.Repositories
{
    // Defines the database operations required for managing
    // delivery addresses belonging to registered customers.
    public interface IUserAddressRepository
    {
        // Returns all saved delivery addresses belonging
        // to the specified customer.
        //
        // The default address will be returned first.
        Task<List<UserAddress>> GetUserAddressesAsync(
            string userId);

        // Returns a specific address only when it belongs
        // to the specified logged-in customer.
        Task<UserAddress?> GetUserAddressByIdAsync(
            int addressId,
            string userId);

        // Adds a new saved delivery address.
        //
        // When the new address is marked as default,
        // the repository will remove the default status
        // from the customer's existing addresses.
        Task<UserAddress> AddUserAddressAsync(
            UserAddress userAddress);

        // Updates an existing address only when it belongs
        // to the specified logged-in customer.
        //
        // When the address is marked as default,
        // the repository will remove the default status
        // from the customer's other addresses.
        Task<UserAddress?> UpdateUserAddressAsync(
            int addressId,
            string userId,
            UserAddress updatedAddress);

        // Deletes an address only when it belongs
        // to the specified logged-in customer.
        //
        // Returns true when the address was deleted.
        Task<bool> DeleteUserAddressAsync(
            int addressId,
            string userId);

        // Marks one address as the customer's default address
        // and removes the default status from all other addresses.
        //
        // Returns the updated address, or null when the address
        // does not exist or belongs to another customer.
        Task<UserAddress?> SetDefaultAddressAsync(
            int addressId,
            string userId);
    }
}