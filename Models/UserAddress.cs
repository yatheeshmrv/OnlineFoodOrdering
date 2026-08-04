using System.Text.Json.Serialization;

namespace FoodOrderAPI.Models
{
    // UserAddress represents a reusable delivery address saved
    // by a registered customer.
    public class UserAddress
    {
        // Primary key for the UserAddresses table.
        public int Id { get; set; }

        // Identity user ID of the customer who owns this address.
        public string UserId { get; set; } = string.Empty;

        // Friendly name shown to the customer.
        // Examples: Home, Work or Other.
        public string AddressLabel { get; set; } = string.Empty;

        // Name of the person receiving the delivery.
        public string RecipientName { get; set; } = string.Empty;

        // Phone number used for delivery communication.
        public string RecipientPhone { get; set; } = string.Empty;

        // House number, building name, street or main address.
        public string AddressLine1 { get; set; } = string.Empty;

        // Apartment, floor, area or additional address information.
        public string? AddressLine2 { get; set; }

        // Nearby location that helps the delivery partner.
        public string? Landmark { get; set; }

        // Delivery city.
        public string City { get; set; } = string.Empty;

        // Delivery state.
        public string State { get; set; } = string.Empty;

        // Six-digit Indian postal or PIN code.
        public string PostalCode { get; set; } = string.Empty;

        // Indicates whether this is the customer's preferred address.
        public bool IsDefault { get; set; }

        // Navigation property for the Identity user who owns
        // this address.
        [JsonIgnore]
        public ApplicationUser? User { get; set; }
    }
}