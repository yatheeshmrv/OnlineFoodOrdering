using FoodOrderAPI.Data;
using FoodOrderAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace FoodOrderAPI.Repositories
{
    // Handles database operations for customer delivery addresses.
    public class UserAddressRepository
        : IUserAddressRepository
    {
        // Used to communicate with the SQL Server database.
        private readonly ApplicationDbContext _context;

        // Receives ApplicationDbContext through dependency injection.
        public UserAddressRepository(
            ApplicationDbContext context)
        {
            _context = context;
        }

        // Returns all saved delivery addresses belonging
        // to the specified customer.
        public async Task<List<UserAddress>>
            GetUserAddressesAsync(
                string userId)
        {
            return await _context.UserAddresses
                .AsNoTracking()
                .Where(address =>
                    address.UserId == userId)
                .OrderByDescending(address =>
                    address.IsDefault)
                .ThenBy(address =>
                    address.AddressLabel)
                .ThenBy(address =>
                    address.Id)
                .ToListAsync();
        }

        // Returns one address only when it belongs
        // to the specified logged-in customer.
        public async Task<UserAddress?>
            GetUserAddressByIdAsync(
                int addressId,
                string userId)
        {
            return await _context.UserAddresses
                .AsNoTracking()
                .FirstOrDefaultAsync(address =>
                    address.Id == addressId &&
                    address.UserId == userId);
        }

        // Adds a new saved delivery address.
        public async Task<UserAddress>
            AddUserAddressAsync(
                UserAddress userAddress)
        {
            // Checks whether the customer already has
            // any saved delivery addresses.
            var hasExistingAddress =
                await _context.UserAddresses
                    .AnyAsync(address =>
                        address.UserId ==
                        userAddress.UserId);

            // The customer's first address automatically
            // becomes the default address.
            if (!hasExistingAddress)
            {
                userAddress.IsDefault = true;
            }

            // When the new address should become the default,
            // remove the default status from existing addresses.
            if (userAddress.IsDefault)
            {
                await RemoveExistingDefaultAddressesAsync(
                    userAddress.UserId);
            }

            // UserId already identifies the existing Identity user.
            // Clearing the navigation property prevents EF Core
            // from attempting to add or modify the user.
            userAddress.User = null;

            // Adds the new address to EF Core tracking.
            await _context.UserAddresses.AddAsync(
                userAddress);

            // Saves the address to the database.
            await _context.SaveChangesAsync();

            return userAddress;
        }

        // Updates an existing address only when it belongs
        // to the specified customer.
        public async Task<UserAddress?>
            UpdateUserAddressAsync(
                int addressId,
                string userId,
                UserAddress updatedAddress)
        {
            // Loads the address while checking ownership.
            var existingAddress =
                await _context.UserAddresses
                    .FirstOrDefaultAsync(address =>
                        address.Id == addressId &&
                        address.UserId == userId);

            // Returns null when the address does not exist
            // or belongs to another customer.
            if (existingAddress == null)
            {
                return null;
            }

            // When this address should become the default,
            // remove the default status from all other
            // addresses before setting this one as default.
            if (updatedAddress.IsDefault)
            {
                await RemoveExistingDefaultAddressesAsync(
                    userId,
                    addressId);
            }

            // Updates only editable address fields.
            // UserId and Id cannot be changed by the client.
            existingAddress.AddressLabel =
                updatedAddress.AddressLabel;

            existingAddress.RecipientName =
                updatedAddress.RecipientName;

            existingAddress.RecipientPhone =
                updatedAddress.RecipientPhone;

            existingAddress.AddressLine1 =
                updatedAddress.AddressLine1;

            existingAddress.AddressLine2 =
                updatedAddress.AddressLine2;

            existingAddress.Landmark =
                updatedAddress.Landmark;

            existingAddress.City =
                updatedAddress.City;

            existingAddress.State =
                updatedAddress.State;

            existingAddress.PostalCode =
                updatedAddress.PostalCode;

            // A non-default address can become the default.
            //
            // An existing default address remains default when
            // IsDefault is false. This prevents the customer
            // from accidentally ending up without a default
            // address while editing its other fields.
            if (updatedAddress.IsDefault)
            {
                existingAddress.IsDefault = true;
            }

            // Saves the updated address.
            await _context.SaveChangesAsync();

            return existingAddress;
        }

        // Deletes an address only when it belongs
        // to the specified customer.
        public async Task<bool>
            DeleteUserAddressAsync(
                int addressId,
                string userId)
        {
            // Loads the address while checking ownership.
            var address =
                await _context.UserAddresses
                    .FirstOrDefaultAsync(
                        existingAddress =>
                            existingAddress.Id ==
                                addressId &&
                            existingAddress.UserId ==
                                userId);

            // Returns false when the address does not exist
            // or belongs to another customer.
            if (address == null)
            {
                return false;
            }

            // Remembers whether a replacement default address
            // needs to be selected after deletion.
            var wasDefaultAddress = address.IsDefault;

            // Marks the address for deletion.
            _context.UserAddresses.Remove(address);

            // Saves the deletion first to avoid a conflict with
            // the unique default-address database index.
            await _context.SaveChangesAsync();

            // When the deleted address was the default,
            // automatically promotes another saved address.
            if (wasDefaultAddress)
            {
                var replacementAddress =
                    await _context.UserAddresses
                        .Where(existingAddress =>
                            existingAddress.UserId ==
                            userId)
                        .OrderBy(existingAddress =>
                            existingAddress.Id)
                        .FirstOrDefaultAsync();

                if (replacementAddress != null)
                {
                    replacementAddress.IsDefault = true;

                    await _context.SaveChangesAsync();
                }
            }

            return true;
        }

        // Marks one address as the customer's default address.
        public async Task<UserAddress?>
            SetDefaultAddressAsync(
                int addressId,
                string userId)
        {
            // Loads the requested address while checking ownership.
            var address =
                await _context.UserAddresses
                    .FirstOrDefaultAsync(
                        existingAddress =>
                            existingAddress.Id ==
                                addressId &&
                            existingAddress.UserId ==
                                userId);

            // Returns null when the address does not exist
            // or belongs to another customer.
            if (address == null)
            {
                return null;
            }

            // No database update is required when the address
            // is already the customer's default.
            if (address.IsDefault)
            {
                return address;
            }

            // Removes the existing default before setting
            // the selected address as the new default.
            await RemoveExistingDefaultAddressesAsync(
                userId,
                addressId);

            address.IsDefault = true;

            await _context.SaveChangesAsync();

            return address;
        }

        // Removes the default status from the customer's existing
        // default address.
        //
        // Saving this change separately prevents a unique-index
        // conflict when another address becomes the default.
        private async Task
            RemoveExistingDefaultAddressesAsync(
                string userId,
                int? excludedAddressId = null)
        {
            var defaultAddresses =
                await _context.UserAddresses
                    .Where(address =>
                        address.UserId == userId &&
                        address.IsDefault &&
                        (!excludedAddressId.HasValue ||
                         address.Id !=
                            excludedAddressId.Value))
                    .ToListAsync();

            if (defaultAddresses.Count == 0)
            {
                return;
            }

            foreach (var address in defaultAddresses)
            {
                address.IsDefault = false;
            }

            await _context.SaveChangesAsync();
        }
    }
}