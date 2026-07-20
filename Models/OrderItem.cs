using System.Text.Json.Serialization;

namespace FoodOrderAPI.Models
{
    public class OrderItem
    {
        public int Id { get; set; }

        // Foreign key for Order.
        // This connects OrderItem to Order in the database.
        public int OrderId { get; set; }

        // Navigation property for Order.
        // Nullable because we do not send full Order object from Postman.
        // JsonIgnore prevents circular reference issues while returning response.
        [JsonIgnore]
        public Order? Order { get; set; }

        // Foreign key for FoodItem.
        // From Postman, we send only foodItemId, not the full FoodItem object.
        public int FoodItemId { get; set; }

        // Navigation property for FoodItem.
        // Nullable because we do not send full FoodItem object from Postman.
        // JsonIgnore prevents circular reference issues while returning response.
        [JsonIgnore]
        public FoodItem? FoodItem { get; set; }

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }
    }
}