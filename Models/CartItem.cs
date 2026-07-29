using System.Text.Json.Serialization;

namespace FoodOrderAPI.Models
{
    // Represents one food item added to a customer's shopping cart.
    public class CartItem
    {
        // Primary key for the CartItems table.
        public int Id { get; set; }

        // Foreign key connecting this item to its cart.
        public int CartId { get; set; }

        // Navigation property for the cart containing this item.
        // JsonIgnore prevents circular references in API responses.
        [JsonIgnore]
        public Cart? Cart { get; set; }

        // Foreign key identifying the selected food item.
        public int FoodItemId { get; set; }

        // Navigation property used to access the food item's
        // name, current price and availability.
        // JsonIgnore prevents the complete FoodItem entity
        // from being included automatically in API responses.
        [JsonIgnore]
        public FoodItem? FoodItem { get; set; }

        // Number of units of this food item in the cart.
        public int Quantity { get; set; }
    }
}