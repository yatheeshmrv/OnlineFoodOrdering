namespace FoodOrderAPI.DTOs
{
    // Contains the information required when a customer
    // converts their shopping cart into an order.
    public class CheckoutDto
    {
        // ID of the saved delivery address selected
        // by the authenticated customer.
        public int UserAddressId { get; set; }

        // Optional instructions that apply only to this order.
        // Examples:
        // "Call when you arrive."
        // "Leave the order with security."
        public string? DeliveryInstructions { get; set; }
    }
}