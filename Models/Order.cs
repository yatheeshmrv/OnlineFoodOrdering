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

        // Date and time when the order was placed.
        public DateTime OrderDate { get; set; } = DateTime.Now;

        // Identity user ID of the customer who placed the order.
        // Nullable so that existing orders are not affected.
        public string? UserId { get; set; }

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