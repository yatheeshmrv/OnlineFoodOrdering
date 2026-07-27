namespace FoodOrderAPI.DTOs
{
    // Represents a paginated API response.
    // T represents the type of item contained in the response.
    public class PagedResponseDto<T>
    {
        // Contains the records for the requested page.
        public List<T> Items { get; set; } = new List<T>();

        // Current page number.
        public int PageNumber { get; set; }

        // Number of records requested per page.
        public int PageSize { get; set; }

        // Total number of records matching the filters.
        public int TotalCount { get; set; }

        // Total number of available pages.
        public int TotalPages { get; set; }

        // Indicates whether a previous page exists.
        public bool HasPreviousPage =>
            PageNumber > 1;

        // Indicates whether another page exists.
        public bool HasNextPage =>
            PageNumber < TotalPages;
    }
}