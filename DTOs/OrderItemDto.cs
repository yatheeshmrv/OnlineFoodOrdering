namespace FoodOrderAPI.DTOs
{
    public class OrderItemDto
    {
        public int FoodItemId { get; set; }

        public string FoodItemName { get; set; } = string.Empty;

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }
    }
}
