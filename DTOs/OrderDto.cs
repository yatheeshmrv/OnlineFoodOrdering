namespace FoodOrderAPI.DTOs
{
    public class OrderDto
    {
        public int Id { get; set; }

        public string CustomerName { get; set; } = string.Empty;

        public string CustomerPhone { get; set; } = string.Empty;

        public decimal TotalAmount { get; set; }

        public string OrderStatus { get; set; } = string.Empty;

        public DateTime OrderDate { get; set; }

        // List of all items in this order
        public List<OrderItemDto> Items { get; set; } = new();
    }
}