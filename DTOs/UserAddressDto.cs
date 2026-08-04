namespace FoodOrderAPI.DTOs
{
    // Represents a saved delivery address returned to the customer.
    public class UserAddressDto
    {
        // Unique identifier of the saved address.
        public int Id { get; set; }

        // Friendly address name such as Home, Work or Other.
        public string AddressLabel { get; set; } = string.Empty;

        // Name of the person receiving the delivery.
        public string RecipientName { get; set; } = string.Empty;

        // Phone number used for delivery communication.
        public string RecipientPhone { get; set; } = string.Empty;

        // House number, building name, street or main address.
        public string AddressLine1 { get; set; } = string.Empty;

        // Apartment, floor, area or additional address details.
        public string? AddressLine2 { get; set; }

        // Nearby location that helps the delivery partner.
        public string? Landmark { get; set; }

        // Delivery city.
        public string City { get; set; } = string.Empty;

        // Delivery state.
        public string State { get; set; } = string.Empty;

        // Postal or PIN code.
        public string PostalCode { get; set; } = string.Empty;

        // Indicates whether this is the customer's preferred address.
        public bool IsDefault { get; set; }
    }
}