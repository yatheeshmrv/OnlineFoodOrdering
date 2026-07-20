namespace FoodOrderAPI.Models
{
    // Order represents the main order placed by a customer
    // One Order can contain many OrderItems
    public class Order
    {
        // Primary key for Orders table
        public int Id { get; set; }

        // Customer name who placed the order
        public string CustomerName { get; set; } = string.Empty;

        // Customer phone number
        public string CustomerPhone { get; set; } = string.Empty;

        // Total amount of the full order
        // Example: Pizza x 2 + Burger x 1 = TotalAmount
        public decimal TotalAmount { get; set; }

        // Current status of the order
        // Later we can move this to OrderStatus master table
        public string OrderStatus { get; set; } = "Pending";

        // Date and time when order was placed
        public DateTime OrderDate { get; set; } = DateTime.Now;

        // Navigation property
        // One Order can have many OrderItems
        // Example: One order can contain Pizza, Burger, Biryani
        public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}