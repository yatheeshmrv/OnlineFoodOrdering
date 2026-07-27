namespace FoodOrderAPI.DTOs
{
    // Contains the search, filtering and pagination values
    // received from the food-items GET endpoint.
    public class FoodItemQueryParametersDto
    {
        // Optional text used to search the food-item
        // name and description.
        public string? Search { get; set; }

        // Optional category ID used to filter food items.
        public int? CategoryId { get; set; }

        // Page number requested by the client.
        // Pagination starts from page 1.
        public int PageNumber { get; set; } = 1;

        // Maximum number of records returned per page.
        public int PageSize { get; set; } = 10;
    }
}