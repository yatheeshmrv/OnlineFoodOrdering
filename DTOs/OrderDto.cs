namespace FoodOrderAPI.DTOs
{
    public class OrderDto
    {
        public int Id { get; set; }

        public string CustomerName { get; set; } = string.Empty;

        public string CustomerPhone { get; set; } = string.Empty;

        public int FoodItemId { get; set; }

        public int Quantity { get; set; }

        public decimal TotalAmount { get; set; }

        public string OrderStatus { get; set; } = string.Empty;

        public DateTime OrderDate { get; set; }
    }
}
