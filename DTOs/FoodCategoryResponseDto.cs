namespace FoodOrderAPI.DTOs
{
    // This DTO is used to send food category data as API response
    // It controls what data the client/Postman will receive
    public class FoodCategoryResponseDto
    {
        // Id is returned in response because client may need it for update/delete/get by id
        public int Id { get; set; }

        // CategoryName is returned to show the category name
        public string CategoryName { get; set; } = string.Empty;

        // IsActive is returned to show whether category is active or inactive
        public bool IsActive { get; set; }
    }
}
