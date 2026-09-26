using BlitzMall_Backend.Data;
using BlitzMall_Backend.DTOs.User;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BlitzMall_Backend.Services
{
    public class ProfileService : IProfileService
    {
        private readonly AppDbContext _db;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ProfileService(AppDbContext db, IHttpContextAccessor httpContextAccessor)
        {
            _db = db;
            _httpContextAccessor = httpContextAccessor;
        }

        private int GetUserId()
        {
            var claim = _httpContextAccessor.HttpContext?
                .User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new UnauthorizedAccessException("Not authenticated.");
            return int.Parse(claim);
        }

        private async Task<ProfileDto> BuildProfileAsync(Models.User user)
        {
            var orders = await _db.Orders
                .Where(o => o.UserId == user.Id)
                .Include(o => o.Payments)
                .ToListAsync();

            var ordersCount = orders.Count;

            var bonuses = orders
                .Where(o => o.Payments != null && o.Payments.Any(p => p.Status == "Completed"))
                .Sum(o => Math.Round(o.TotalAmount * 0.02m, 0));

            string level;
            int progress;
            string hint;

            if (bonuses < 1000)
            {
                level = "Бронзовий";
                progress = (int)Math.Min(100, bonuses / 1000m * 100);
                hint = "До наступного рівня - ще декілька покупок";
            }
            else if (bonuses < 3000)
            {
                level = "Срібний";
                progress = (int)Math.Min(100, (bonuses - 1000) / 2000m * 100);
                hint = "До наступного рівня - ще декілька покупок";
            }
            else
            {
                level = "Золотий";
                progress = 100;
                hint = "Ви досягли найвищого рівня!";
            }

            return new ProfileDto
            {
                Id = user.Id,
                Name = user.Name ?? string.Empty,
                Email = user.Email,
                Phone = user.Phone,
                BirthDate = user.BirthDate,
                CreatedAt = user.CreatedAt,
                OrdersCount = ordersCount,
                Bonuses = bonuses,
                Level = level,
                LevelProgressPercent = progress,
                NextLevelHint = hint,
            };
        }

        public async Task<ProfileDto> GetMyProfileAsync()
        {
            var userId = GetUserId();
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId)
                ?? throw new InvalidOperationException("User not found.");

            return await BuildProfileAsync(user);
        }

        public async Task<ProfileDto> UpdateMyProfileAsync(UpdateProfileDto dto)
        {
            var userId = GetUserId();
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId)
                ?? throw new InvalidOperationException("User not found.");

            if (dto.Name != null) user.Name = dto.Name;
            if (dto.Phone != null) user.Phone = dto.Phone;
            if (dto.BirthDate.HasValue) user.BirthDate = dto.BirthDate;
            user.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return await BuildProfileAsync(user);
        }
    }
}
