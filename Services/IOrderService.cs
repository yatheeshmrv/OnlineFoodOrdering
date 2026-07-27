using FoodOrderAPI.DTOs;

namespace FoodOrderAPI.Services
{
    public interface IOrderService
    {
        Task<List<OrderDto>> GetAllOrdersAsync();

        Task<OrderDto?> GetOrderByIdAsync(int id);

        Task<CreateOrderResponseDto> CreateOrderAsync(CreateOrderDto createOrderDto,string userId);

        Task<IEnumerable<OrderDto>> GetMyOrdersAsync(string userId);

        // Returns one order only if it belongs to the logged-in customer.
        Task<OrderDto?> GetMyOrderByIdAsync(int id, string userId);

        Task<OrderDto?> UpdateOrderStatusAsync(
            int id,
            UpdateOrderStatusDto statusDto);

        Task<bool> DeleteOrderAsync(int id);
    }
}