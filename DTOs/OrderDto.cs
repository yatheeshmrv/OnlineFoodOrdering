namespace FoodOrderAPI.DTOs
{
    // Represents an order returned to customers and administrators.
    public class OrderDto
    {
        // Unique identifier of the order.
        public int Id { get; set; }

        // Name of the customer who placed the order.
        public string CustomerName { get; set; } = string.Empty;

        // Phone number from the customer's registered profile.
        public string CustomerPhone { get; set; } = string.Empty;

        // Total amount of the complete order.
        public decimal TotalAmount { get; set; }

        // Current order status.
        // Examples: Pending, Confirmed, Preparing or Delivered.
        public string OrderStatus { get; set; } = string.Empty;

        // Date and time when the order was placed.
        public DateTime OrderDate { get; set; }

        /*
         * Delivery-address snapshot
         *
         * These values represent the address selected when
         * the order was placed.
         *
         * They remain unchanged even when the customer later
         * edits or deletes the original saved address.
         *
         * They are nullable because historical orders created
         * before address support do not contain these values.
         */

        // Name of the person receiving the order.
        public string? DeliveryRecipientName { get; set; }

        // Phone number used for delivery communication.
        public string? DeliveryPhone { get; set; }

        // House number, building name, street or main address.
        public string? DeliveryAddressLine1 { get; set; }

        // Apartment, floor, area or additional address details.
        public string? DeliveryAddressLine2 { get; set; }

        // Nearby location that helps the delivery partner.
        public string? DeliveryLandmark { get; set; }

        // Delivery city.
        public string? DeliveryCity { get; set; }

        // Delivery state.
        public string? DeliveryState { get; set; }

        // Delivery postal or PIN code.
        public string? DeliveryPostalCode { get; set; }

        // Optional instructions provided for this order.
        public string? DeliveryInstructions { get; set; }

        // List of all food items included in this order.
        public List<OrderItemDto> Items { get; set; } = new();
    }
}