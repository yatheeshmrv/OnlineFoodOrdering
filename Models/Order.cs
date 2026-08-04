using System.Text.Json.Serialization;

namespace FoodOrderAPI.Models
{
    // Order represents the main order placed by a customer.
    // One Order can contain many OrderItems.
    public class Order
    {
        // Primary key for the Orders table.
        public int Id { get; set; }

        // Name of the customer who placed the order.
        public string CustomerName { get; set; } = string.Empty;

        // Customer phone number.
        public string CustomerPhone { get; set; } = string.Empty;

        // Total amount of the complete order.
        // Example: Pizza x 2 + Burger x 1 = TotalAmount.
        public decimal TotalAmount { get; set; }

        // Current status of the order.
        // Example: Pending, Confirmed, Preparing, Delivered.
        public string OrderStatus { get; set; } = "Pending";

        // Payment method selected during checkout.
        public string PaymentMethod { get; set; } =
            PaymentMethods.CashOnDelivery;

        // Current payment-processing state of the order.
        public string PaymentStatus { get; set; } =
            PaymentStatuses.Pending;

        // Date and time when the order was placed.
        public DateTime OrderDate { get; set; } = DateTime.Now;

        // Identity user ID of the customer who placed the order.
        // Nullable so that existing orders are not affected.
        public string? UserId { get; set; }

        /*
         * Delivery-address snapshot
         *
         * These values are copied from the selected saved address
         * when the customer places the order.
         *
         * They are not updated when the customer later edits or
         * deletes the original saved address.
         *
         * The fields are nullable so existing database orders can
         * remain valid after the delivery-address migration.
         */

        // Name of the person receiving the order.
        public string? DeliveryRecipientName { get; set; }

        // Phone number used for delivery communication.
        public string? DeliveryPhone { get; set; }

        // House number, building name, street or primary address.
        public string? DeliveryAddressLine1 { get; set; }

        // Apartment, floor, area or additional address information.
        public string? DeliveryAddressLine2 { get; set; }

        // Nearby location that helps the delivery partner.
        public string? DeliveryLandmark { get; set; }

        // Delivery city.
        public string? DeliveryCity { get; set; }

        // Delivery state.
        public string? DeliveryState { get; set; }

        // Delivery postal or PIN code.
        public string? DeliveryPostalCode { get; set; }

        // Optional instructions provided specifically for this order.
        public string? DeliveryInstructions { get; set; }

        // Navigation property for the registered Identity user.
        // JsonIgnore prevents unnecessary user information
        // from being included in API responses.
        [JsonIgnore]
        public ApplicationUser? User { get; set; }

        // Navigation property.
        // One Order can contain many OrderItems.
        public List<OrderItem> OrderItems { get; set; } =
            new List<OrderItem>();
    }
}