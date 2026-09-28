using BlitzMall_Backend.DTOs.Delivery;

namespace BlitzMall_Backend.Services
{
    public interface IDeliveryService
    {
        Task<List<DeliveryDto>> GetAllAsync();
        Task<DeliveryDto?> GetByIdAsync(int id);
        Task<DeliveryDto?> CreateAsync(CreateDeliveryDto dto);
        Task<DeliveryDto?> UpdateAsync(int id, UpdateDeliveryDto dto);
        Task<bool> DeleteAsync(int id);
    }
}