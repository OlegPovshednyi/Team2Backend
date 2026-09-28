using BlitzMall_Backend.Data;
using BlitzMall_Backend.DTOs.Delivery;
using BlitzMall_Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace BlitzMall_Backend.Services
{
    public class DeliveryService : IDeliveryService
    {
        private readonly AppDbContext _db;

        public DeliveryService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<DeliveryDto>> GetAllAsync()
        {
            return await _db.Deliveries
                .Select(d => new DeliveryDto
                {
                    Id = d.Id,
                    OrderId = d.OrderId,
                    Method = d.Method,
                    Carrier = d.Carrier,
                    TrackingNumber = d.TrackingNumber,
                    Status = d.Status,
                    CreatedAt = d.CreatedAt,
                    ShippedAt = d.ShippedAt,
                    DeliveredAt = d.DeliveredAt
                })
                .ToListAsync();
        }

        public async Task<DeliveryDto?> GetByIdAsync(int id)
        {
            return await _db.Deliveries
                .Where(d => d.Id == id)
                .Select(d => new DeliveryDto
                {
                    Id = d.Id,
                    OrderId = d.OrderId,
                    Method = d.Method,
                    Carrier = d.Carrier,
                    TrackingNumber = d.TrackingNumber,
                    Status = d.Status,
                    CreatedAt = d.CreatedAt,
                    ShippedAt = d.ShippedAt,
                    DeliveredAt = d.DeliveredAt
                })
                .FirstOrDefaultAsync();
        }

        public async Task<DeliveryDto?> CreateAsync(CreateDeliveryDto dto)
        {
            var order = await _db.Orders
                .FirstOrDefaultAsync(o => o.Id == dto.OrderId);

            if (order == null)
            {
                return null;
            }

            var delivery = new Delivery
            {
                OrderId = dto.OrderId,
                Method = dto.Method,
                Carrier = dto.Carrier,
                TrackingNumber = dto.TrackingNumber,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            _db.Deliveries.Add(delivery);

            await _db.SaveChangesAsync();

            return await GetByIdAsync(delivery.Id);
        }

        public async Task<DeliveryDto?> UpdateAsync(
            int id,
            UpdateDeliveryDto dto)
        {
            var delivery = await _db.Deliveries
                .FirstOrDefaultAsync(d => d.Id == id);

            if (delivery == null)
            {
                return null;
            }

            delivery.Status = dto.Status;
            delivery.Carrier = dto.Carrier;
            delivery.TrackingNumber = dto.TrackingNumber;
            delivery.ShippedAt = dto.ShippedAt;
            delivery.DeliveredAt = dto.DeliveredAt;

            await _db.SaveChangesAsync();

            return await GetByIdAsync(delivery.Id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var delivery = await _db.Deliveries
                .FirstOrDefaultAsync(d => d.Id == id);

            if (delivery == null)
            {
                return false;
            }

            _db.Deliveries.Remove(delivery);

            await _db.SaveChangesAsync();

            return true;
        }
    }
}