using BlitzMall_Backend.Data;
using BlitzMall_Backend.DTOs.Payment;
using BlitzMall_Backend.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BlitzMall_Backend.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly AppDbContext _db;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PaymentService(
            AppDbContext db,
            IHttpContextAccessor httpContextAccessor)
        {
            _db = db;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<List<PaymentDto>> GetAllAsync()
        {
            return await _db.Payments
                .Select(p => new PaymentDto
                {
                    Id = p.Id,
                    OrderId = p.OrderId,
                    Amount = p.Amount,
                    Status = p.Status,
                    Method = p.Method,
                    TransactionId = p.TransactionId,
                    CreatedAt = p.CreatedAt,
                    CompletedAt = p.CompletedAt
                })
                .ToListAsync();
        }

        public async Task<PaymentDto?> GetByIdAsync(int id)
        {
            return await _db.Payments
                .Where(p => p.Id == id)
                .Select(p => new PaymentDto
                {
                    Id = p.Id,
                    OrderId = p.OrderId,
                    Amount = p.Amount,
                    Status = p.Status,
                    Method = p.Method,
                    TransactionId = p.TransactionId,
                    CreatedAt = p.CreatedAt,
                    CompletedAt = p.CompletedAt
                })
                .FirstOrDefaultAsync();
        }

        public async Task<PaymentDto?> CreateAsync(
            CreatePaymentDto dto)
        {
            var userId = GetUserId();

            var order = await _db.Orders
                .FirstOrDefaultAsync(o =>
                    o.Id == dto.OrderId &&
                    o.UserId == userId);

            if (order == null)
                return null;

            if (order.OrderStatus == "Paid")
            {
                throw new InvalidOperationException(
                    "Order is already paid.");
            }

            var existingPayment = await _db.Payments
                .AnyAsync(p =>
                    p.OrderId == dto.OrderId &&
                    p.Status == "Completed");

            if (existingPayment)
            {
                throw new InvalidOperationException(
                    "Order is already paid.");
            }

            var payment = new Payment
            {
                OrderId = order.Id,

                // Сума береться з Order, а не від клієнта.
                Amount = order.TotalAmount,

                Method = dto.Method,
                Status = "Pending",

                TransactionId =
                    $"SIM-{Guid.NewGuid():N}"
                    .Substring(0, 12)
                    .ToUpper(),

                CreatedAt = DateTime.UtcNow
            };

            _db.Payments.Add(payment);

            await _db.SaveChangesAsync();

            return await GetByIdAsync(payment.Id);
        }

        public async Task<PaymentDto?> ProcessAsync(
            int paymentId)
        {
            var userId = GetUserId();

            var payment = await _db.Payments
                .Include(p => p.Order)
                .FirstOrDefaultAsync(p =>
                    p.Id == paymentId &&
                    p.Order!.UserId == userId);

            if (payment == null)
                return null;

            if (payment.Status == "Completed")
            {
                throw new InvalidOperationException(
                    "Payment is already completed.");
            }

            if (payment.Status != "Pending")
            {
                throw new InvalidOperationException(
                    "Payment cannot be processed.");
            }

            // Імітація успішної оплати.
            payment.Status = "Completed";
            payment.CompletedAt = DateTime.UtcNow;

            if (payment.Order != null)
            {
                payment.Order.OrderStatus = "Paid";
                payment.Order.UpdatedDate = DateTime.UtcNow;
            }

            await _db.SaveChangesAsync();

            return await GetByIdAsync(payment.Id);
        }

        public async Task<PaymentDto?> UpdateAsync(
            int id,
            UpdatePaymentDto dto)
        {
            var payment = await _db.Payments
                .FirstOrDefaultAsync(p => p.Id == id);

            if (payment == null)
                return null;

            payment.Status = dto.Status;
            payment.CompletedAt = dto.CompletedAt;

            await _db.SaveChangesAsync();

            return await GetByIdAsync(payment.Id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var payment = await _db.Payments
                .FirstOrDefaultAsync(p => p.Id == id);

            if (payment == null)
                return false;

            _db.Payments.Remove(payment);

            await _db.SaveChangesAsync();

            return true;
        }

        private int GetUserId()
        {
            var userId = _httpContextAccessor.HttpContext?
                .User
                .FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userId, out var id))
            {
                throw new UnauthorizedAccessException(
                    "Invalid user identity.");
            }

            return id;
        }
    }
}