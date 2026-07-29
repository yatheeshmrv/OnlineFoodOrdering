namespace FoodOrderAPI.DTOs
{
    // Represents the complete shopping cart returned to the customer.
    public class CartDto
    {
        // ID of the customer's cart.
        public int Id { get; set; }

        // All food items currently present in the cart.
        public List<CartItemDto> Items { get; set; } =
            new List<CartItemDto>();

        // Calculated total of all item subtotals.
        // This value is not stored in the database.
        public decimal TotalAmount { get; set; }
    }
}