namespace FoodOrderAPI.DTOs
{
    // Contains the new quantity for an existing cart item.
    public class UpdateCartItemQuantityDto
    {
        // New total quantity that should be stored
        // for the selected cart item.
        public int Quantity { get; set; }
    }
}