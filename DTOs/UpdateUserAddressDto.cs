namespace FoodOrderAPI.DTOs
{
    // Contains the information required when a customer
    // updates an existing saved delivery address.
    public class UpdateUserAddressDto
    {
        // Friendly name shown to the customer.
        // Examples: Home, Work or Other.
        public string AddressLabel { get; set; } = string.Empty;

        // Name of the person receiving the delivery.
        public string RecipientName { get; set; } = string.Empty;

        // Phone number used by the delivery partner.
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

        // Indicates whether this address should become the
        // customer's preferred delivery address.
        public bool IsDefault { get; set; }
    }
}