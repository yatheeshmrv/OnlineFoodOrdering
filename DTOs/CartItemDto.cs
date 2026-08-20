namespace FoodOrderAPI.DTOs
{
    // Represents one food item returned in the customer's cart.
    public class CartItemDto
    {
        // ID of the CartItem record.
        // This ID is used when updating or removing the cart item.
        public int Id { get; set; }

        // ID of the selected food item.
        public int FoodItemId { get; set; }

        // Name of the selected food item.
        public string FoodItemName { get; set; } = string.Empty;

        // Image URL/path of the selected food item.
        public string ImageUrl { get; set; } = string.Empty;

        // Current price of one unit of the food item.
        public decimal UnitPrice { get; set; }

        // Number of units currently in the cart.
        public int Quantity { get; set; }

        // Indicates whether the food item is currently available.
        public bool IsAvailable { get; set; }

        // Calculated value:
        // UnitPrice multiplied by Quantity.
        public decimal Subtotal { get; set; }
    }
}