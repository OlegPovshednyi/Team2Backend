using BlitzMall_Backend.Data;
using BlitzMall_Backend.DTOs.Review;
using BlitzMall_Backend.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BlitzMall_Backend.Services
{
    public class ReviewService : IReviewService
    {
        private readonly AppDbContext _db;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ReviewService(
            AppDbContext db,
            IHttpContextAccessor httpContextAccessor)
        {
            _db = db;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<List<ReviewDto>> GetAllAsync(int? productId = null)
        {
            return await _db.Reviews
                .Where(r => productId == null || r.ProductId == productId)
                .Select(r => new ReviewDto
                {
                    Id = r.Id,
                    UserId = r.UserId,
                    ProductId = r.ProductId,
                    Text = r.Text,
                    CreatedDate = r.CreatedDate,
                    UpdatedDate = r.UpdatedDate,
                    Rating = r.Rating
                })
                .ToListAsync();
        }

        public async Task<ReviewDto?> GetByIdAsync(int id)
        {
            return await _db.Reviews
                .Where(r => r.Id == id)
                .Select(r => new ReviewDto
                {
                    Id = r.Id,
                    UserId = r.UserId,
                    ProductId = r.ProductId,
                    Text = r.Text,
                    CreatedDate = r.CreatedDate,
                    UpdatedDate = r.UpdatedDate,
                    Rating = r.Rating
                })
                .FirstOrDefaultAsync();
        }

        public async Task<ReviewDto> CreateAsync(CreateReviewDto dto)
        {
            var userIdClaim = _httpContextAccessor.HttpContext?
                .User
                .FindFirstValue(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                throw new UnauthorizedAccessException();
            }

            int userId = int.Parse(userIdClaim);

            var existingReview = await _db.Reviews
                .AnyAsync(r =>
                    r.UserId == userId &&
                    r.ProductId == dto.ProductId);

            if (existingReview)
            {
                throw new InvalidOperationException(
                    "You have already reviewed this product.");
            }

            var productExists = await _db.Products
                .AnyAsync(p => p.Id == dto.ProductId);

            if (!productExists)
            {
                throw new InvalidOperationException(
                    "Product not found.");
            }

            var review = new Review
            {
                UserId = userId,
                ProductId = dto.ProductId,
                Text = dto.Text,
                Rating = dto.Rating,
                CreatedDate = DateTime.UtcNow
            };

            _db.Reviews.Add(review);

            await _db.SaveChangesAsync();

            return new ReviewDto
            {
                Id = review.Id,
                UserId = review.UserId,
                ProductId = review.ProductId,
                Text = review.Text,
                CreatedDate = review.CreatedDate,
                UpdatedDate = review.UpdatedDate,
                Rating = review.Rating
            };
        }

        public async Task<ReviewDto?> UpdateAsync(
            int id,
            UpdateReviewDto dto)
        {
            var review = await _db.Reviews
                .FirstOrDefaultAsync(r => r.Id == id);

            if (review == null)
            {
                return null;
            }

            var userIdClaim = _httpContextAccessor.HttpContext?
                .User
                .FindFirstValue(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                throw new UnauthorizedAccessException();
            }

            int userId = int.Parse(userIdClaim);

            if (review.UserId != userId)
            {
                throw new UnauthorizedAccessException(
                    "You cannot update this review.");
            }

            review.Text = dto.Text;
            review.Rating = dto.Rating;
            review.UpdatedDate = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return new ReviewDto
            {
                Id = review.Id,
                UserId = review.UserId,
                ProductId = review.ProductId,
                Text = review.Text,
                CreatedDate = review.CreatedDate,
                UpdatedDate = review.UpdatedDate,
                Rating = review.Rating
            };
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var review = await _db.Reviews
                .FirstOrDefaultAsync(r => r.Id == id);

            if (review == null)
            {
                return false;
            }

            var userIdClaim = _httpContextAccessor.HttpContext?
                .User
                .FindFirstValue(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                throw new UnauthorizedAccessException();
            }

            int userId = int.Parse(userIdClaim);

            if (review.UserId != userId)
            {
                throw new UnauthorizedAccessException(
                    "You cannot delete this review.");
            }

            _db.Reviews.Remove(review);

            await _db.SaveChangesAsync();

            return true;
        }
    }
}
