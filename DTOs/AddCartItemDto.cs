namespace FoodOrderAPI.DTOs
{
    // Contains the information required to add
    // a food item to the logged-in customer's cart.
    public class AddCartItemDto
    {
        // ID of the food item being added.
        public int FoodItemId { get; set; }

        // Number of units to add to the cart.
        public int Quantity { get; set; }
    }
}