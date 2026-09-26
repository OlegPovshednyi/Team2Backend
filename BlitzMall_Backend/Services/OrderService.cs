using BlitzMall_Backend.Data;
using BlitzMall_Backend.DTOs.Order;
using BlitzMall_Backend.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BlitzMall_Backend.Services
{
    public class OrderService : IOrderService
    {
        private readonly AppDbContext _db;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public OrderService(AppDbContext db, IHttpContextAccessor httpContextAccessor)
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

        public async Task<OrderDto> CreateFromCartAsync(CreateOrderDto dto)
        {
            var userId = GetUserId();

            var cart = await _db.Carts
                .Include(c => c.CartItems!)
                    .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || cart.CartItems == null || !cart.CartItems.Any())
                throw new InvalidOperationException("Cart is empty.");

            var address = new Address
            {
                UserId = userId,
                Street = dto.DeliveryAddress,
                City = "",
                Country = "",
                Region = "",
                PostalCode = "",
                Apartment = "",
                BuildingNumber = ""
            };

            _db.Addresses.Add(address);
            await _db.SaveChangesAsync();

            var total = cart.CartItems.Sum(ci => ci.Quantity * (ci.Product?.Price ?? ci.UnitPrice));

            var order = new Order
            {
                UserId = userId,
                TotalAmount = total,
                OrderStatus = "Pending",
                CreatedDate = DateTime.UtcNow,
                AddressId = address.Id,
                Phone = dto.Phone,
                Comment = dto.Comment
            };

            _db.Orders.Add(order);
            await _db.SaveChangesAsync();

            var orderItems = cart.CartItems.Select(ci => new OrderItem
            {
                OrderId = order.Id,
                ProductId = ci.ProductId,
                Quantity = ci.Quantity,
                UnitPrice = ci.Product?.Price ?? ci.UnitPrice
            }).ToList();

            _db.OrderItems.AddRange(orderItems);
            _db.CartItems.RemoveRange(cart.CartItems);

            var payment = new Payment
            {
                OrderId = order.Id,
                Amount = total,
                Method = string.IsNullOrWhiteSpace(dto.PaymentMethod) ? "card" : dto.PaymentMethod,
                Status = "Completed",
                TransactionId = Guid.NewGuid().ToString("N"),
                CreatedAt = DateTime.UtcNow,
                CompletedAt = DateTime.UtcNow,
            };
            _db.Payments.Add(payment);

            order.OrderStatus = "Paid";

            await _db.SaveChangesAsync();

            return new OrderDto
            {
                Id = order.Id,
                OrderStatus = order.OrderStatus ?? string.Empty,
                TotalAmount = order.TotalAmount,
                DeliveryAddress = dto.DeliveryAddress,
                Phone = dto.Phone,
                Comment = dto.Comment,
                PaymentMethod = payment.Method,
                CreatedDate = order.CreatedDate,
                Items = orderItems.Select(oi => new OrderItemDto
                {
                    ProductId = oi.ProductId,
                    ProductName = cart.CartItems
                        .FirstOrDefault(ci => ci.ProductId == oi.ProductId)?.Product?.Name ?? string.Empty,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    Total = oi.Quantity * oi.UnitPrice
                }).ToList()
            };
        }

        public async Task<List<OrderDto>> GetMyOrdersAsync()
        {
            var userId = GetUserId();

            var orders = await _db.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.Address)
                .Include(o => o.Payments)
                .Include(o => o.OrderItems!)
                    .ThenInclude(oi => oi.Product)
                .ToListAsync();

            return orders.Select(o => new OrderDto
            {
                Id = o.Id,
                OrderStatus = o.OrderStatus ?? string.Empty,
                TotalAmount = o.TotalAmount,
                DeliveryAddress = o.Address?.Street ?? string.Empty,
                Phone = o.Phone ?? string.Empty,
                Comment = o.Comment,
                PaymentMethod = o.Payments?.FirstOrDefault()?.Method,
                CreatedDate = o.CreatedDate,
                Items = o.OrderItems?.Select(oi => new OrderItemDto
                {
                    ProductId = oi.ProductId,
                    ProductName = oi.Product?.Name ?? string.Empty,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    Total = oi.Quantity * oi.UnitPrice
                }).ToList() ?? new List<OrderItemDto>()
            }).ToList();
        }
    }
}
