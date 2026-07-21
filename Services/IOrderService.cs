using FoodOrderAPI.DTOs;

namespace FoodOrderAPI.Services
{
    public interface IOrderService
    {
        Task<List<OrderDto>> GetAllOrdersAsync();

        Task<OrderDto?> GetOrderByIdAsync(int id);

        Task<CreateOrderResponseDto> CreateOrderAsync(
            CreateOrderDto createOrderDto);

        Task<OrderDto?> UpdateOrderStatusAsync(
            int id,
            UpdateOrderStatusDto statusDto);

        Task<bool> DeleteOrderAsync(int id);
    }
}