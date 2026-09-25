using BlitzMall_Backend.DTOs.Review;

namespace BlitzMall_Backend.Services
{
    public interface IReviewService
    {
        Task<List<ReviewDto>> GetAllAsync(int? productId = null);
        Task<ReviewDto?> GetByIdAsync(int id);
        Task<ReviewDto> CreateAsync(CreateReviewDto dto);
        Task<ReviewDto?> UpdateAsync(int id, UpdateReviewDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
