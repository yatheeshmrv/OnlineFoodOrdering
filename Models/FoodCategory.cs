namespace FoodOrderAPI.Models
{
    public class FoodCategory
    {
        public int Id { get; set; }

        public string CategoryName { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }
}
