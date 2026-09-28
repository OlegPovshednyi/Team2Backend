using BlitzMall_Backend.DTOs.Payment;

namespace BlitzMall_Backend.Services
{
    public interface IPaymentService
    {
        Task<List<PaymentDto>> GetAllAsync();

        Task<PaymentDto?> GetByIdAsync(int id);

        Task<PaymentDto?> CreateAsync(
            CreatePaymentDto dto);

        Task<PaymentDto?> ProcessAsync(
            int paymentId);

        Task<PaymentDto?> UpdateAsync(
            int id,
            UpdatePaymentDto dto);

        Task<bool> DeleteAsync(int id);
    }
}