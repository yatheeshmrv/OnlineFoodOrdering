using System.Text.Json.Serialization;

namespace FoodOrderAPI.Models
{
    // Represents the shopping cart belonging to a logged-in customer.
    // Each customer can have only one cart.
    public class Cart
    {
        // Primary key for the Carts table.
        public int Id { get; set; }

        // Identity user ID of the customer who owns this cart.
        // This will be configured as a unique foreign key,
        // ensuring that one customer cannot have multiple carts.
        public string UserId { get; set; } = string.Empty;

        // Navigation property for the registered customer.
        // JsonIgnore prevents Identity-user information
        // from being returned in cart API responses.
        [JsonIgnore]
        public ApplicationUser? User { get; set; }

        // Contains all food items currently added to this cart.
        // One Cart can contain many CartItems.
        public List<CartItem> CartItems { get; set; } =
            new List<CartItem>();
    }
}