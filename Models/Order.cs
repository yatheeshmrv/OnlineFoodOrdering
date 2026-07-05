namespace FoodOrderAPI.Models
{
    public class Order
    {
        public int Id { get; set; }

        public string CustomerName { get; set; } = string.Empty;

        public string CustomerPhone { get; set; } = string.Empty;

        public int FoodItemId { get; set; }

        public int Quantity { get; set; }

        public decimal TotalAmount { get; set; }

        public string OrderStatus { get; set; } = "Pending";

        public DateTime OrderDate { get; set; } = DateTime.Now;
    }
}
