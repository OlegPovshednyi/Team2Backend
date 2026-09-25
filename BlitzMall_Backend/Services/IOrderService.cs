using BlitzMall_Backend.DTOs.Order;

namespace BlitzMall_Backend.Services
{
    public interface IOrderService
    {
        Task<OrderDto> CreateFromCartAsync(CreateOrderDto dto);
        Task<List<OrderDto>> GetMyOrdersAsync();
    }
}
