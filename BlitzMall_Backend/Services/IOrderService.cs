using BlitzMall_Backend.DTOs.Order;

namespace BlitzMall_Backend.Services
{
    public interface IOrderService
    {
        Task<List<OrderDto>> GetAllAsync();
        Task<OrderDto?> GetByIdAsync(int id);
        Task<OrderDto?> CreateAsync(CreateOrderDto dto);
        Task<OrderDto> CreateFromCartAsync(CreateOrderDto dto);
        Task<List<OrderDto>> GetMyOrdersAsync();

        Task<OrderDto?> UpdateStatusAsync(
            int id,
            UpdateOrderStatusDto dto);

        Task<bool> DeleteAsync(int id);
    }
}