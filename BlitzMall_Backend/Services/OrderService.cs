using System.Security.Claims;
using BlitzMall_Backend.Data;
using BlitzMall_Backend.DTOs.Order;
using BlitzMall_Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace BlitzMall_Backend.Services
{
    public class OrderService : IOrderService
    {
        private readonly AppDbContext _db;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public OrderService(
            AppDbContext db,
            IHttpContextAccessor httpContextAccessor)
        {
            _db = db;
            _httpContextAccessor = httpContextAccessor;
        }

        private int? GetUserId()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?
                .User.FindFirstValue(ClaimTypes.NameIdentifier);

            return int.TryParse(userIdClaim, out var userId)
                ? userId
                : null;
        }

        public async Task<List<OrderDto>> GetAllAsync()
        {
            return await _db.Orders
                .Include(o => o.Address)
                .Include(o => o.OrderItems!)
                    .ThenInclude(i => i.Product)
                .Select(o => new OrderDto
                {
                    Id = o.Id,
                    UserId = o.UserId,
                    TotalAmount = o.TotalAmount,
                    OrderStatus = o.OrderStatus ?? string.Empty,
                    DeliveryAddress = o.Address != null
                        ? o.Address.Street ?? string.Empty
                        : string.Empty,
                    Phone = o.Phone ?? string.Empty,
                    Comment = o.Comment,
                    CreatedDate = o.CreatedDate,
                    UpdatedDate = o.UpdatedDate,
                    AddressId = o.AddressId,
                    Items = o.OrderItems!
                        .Select(i => new OrderItemDto
                        {
                            Id = i.Id,
                            OrderId = i.OrderId,
                            ProductId = i.ProductId,
                            ProductName = i.Product != null
                                ? i.Product.Name ?? string.Empty
                                : string.Empty,
                            Quantity = i.Quantity,
                            UnitPrice = i.UnitPrice,
                            Total = i.Quantity * i.UnitPrice
                        })
                        .ToList()
                })
                .ToListAsync();
        }

        public async Task<OrderDto?> GetByIdAsync(int id)
        {
            var userId = GetUserId();

            if (userId == null)
                return null;

            var isAdmin = _httpContextAccessor.HttpContext?
                .User.IsInRole("Admin") ?? false;

            return await _db.Orders
                .Include(o => o.Address)
                .Include(o => o.OrderItems!)
                    .ThenInclude(i => i.Product)
                .Where(o =>
                    o.Id == id &&
                    (isAdmin || o.UserId == userId.Value))
                .Select(o => new OrderDto
                {
                    Id = o.Id,
                    UserId = o.UserId,
                    TotalAmount = o.TotalAmount,
                    OrderStatus = o.OrderStatus ?? string.Empty,
                    DeliveryAddress = o.Address != null
                        ? o.Address.Street ?? string.Empty
                        : string.Empty,
                    Phone = o.Phone ?? string.Empty,
                    Comment = o.Comment,
                    CreatedDate = o.CreatedDate,
                    UpdatedDate = o.UpdatedDate,
                    AddressId = o.AddressId,
                    Items = o.OrderItems!
                        .Select(i => new OrderItemDto
                        {
                            Id = i.Id,
                            OrderId = i.OrderId,
                            ProductId = i.ProductId,
                            ProductName = i.Product != null
                                ? i.Product.Name ?? string.Empty
                                : string.Empty,
                            Quantity = i.Quantity,
                            UnitPrice = i.UnitPrice,
                            Total = i.Quantity * i.UnitPrice
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();
        }

        public async Task<OrderDto?> CreateAsync(CreateOrderDto dto)
        {
            return await CreateFromCartInternalAsync(dto);
        }

        public async Task<OrderDto> CreateFromCartAsync(
            CreateOrderDto dto)
        {
            var order = await CreateFromCartInternalAsync(dto);

            if (order == null)
                throw new InvalidOperationException(
                    "Unable to create order.");

            return order;
        }

        private async Task<OrderDto?> CreateFromCartInternalAsync(
            CreateOrderDto dto)
        {
            var userId = GetUserId();

            if (userId == null)
                return null;

            var cart = await _db.Carts
                .Include(c => c.CartItems!)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId.Value);

            if (cart?.CartItems == null || !cart.CartItems.Any())
                return null;

            Address? address;

            if (dto.AddressId.HasValue)
            {
                address = await _db.Addresses
                    .FirstOrDefaultAsync(a =>
                        a.Id == dto.AddressId.Value &&
                        a.UserId == userId.Value);

                if (address == null)
                    return null;
            }
            else
            {
                address = new Address
                {
                    UserId = userId.Value,
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
            }

            var order = new Order
            {
                UserId = userId.Value,
                AddressId = address.Id,
                OrderStatus = "Pending",
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow,
                TotalAmount = 0,
                Phone = dto.Phone,
                Comment = dto.Comment,
                OrderItems = new List<OrderItem>()
            };

            foreach (var cartItem in cart.CartItems)
            {
                var unitPrice =
                    cartItem.Product?.Price ?? cartItem.UnitPrice;

                var orderItem = new OrderItem
                {
                    ProductId = cartItem.ProductId,
                    Quantity = cartItem.Quantity,
                    UnitPrice = unitPrice
                };

                order.OrderItems.Add(orderItem);

                order.TotalAmount +=
                    orderItem.Quantity * orderItem.UnitPrice;
            }

            _db.Orders.Add(order);
            _db.CartItems.RemoveRange(cart.CartItems);

            cart.UpdatedDate = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return await GetByIdAsync(order.Id);
        }

        public async Task<List<OrderDto>> GetMyOrdersAsync()
        {
            var userId = GetUserId();

            if (userId == null)
                throw new UnauthorizedAccessException(
                    "Not authenticated.");

            return await _db.Orders
                .Include(o => o.Address)
                .Include(o => o.OrderItems!)
                    .ThenInclude(i => i.Product)
                .Where(o => o.UserId == userId.Value)
                .OrderByDescending(o => o.CreatedDate)
                .Select(o => new OrderDto
                {
                    Id = o.Id,
                    UserId = o.UserId,
                    TotalAmount = o.TotalAmount,
                    OrderStatus = o.OrderStatus ?? string.Empty,
                    DeliveryAddress = o.Address != null
                        ? o.Address.Street ?? string.Empty
                        : string.Empty,
                    Phone = o.Phone ?? string.Empty,
                    Comment = o.Comment,
                    CreatedDate = o.CreatedDate,
                    UpdatedDate = o.UpdatedDate,
                    AddressId = o.AddressId,
                    Items = o.OrderItems!
                        .Select(i => new OrderItemDto
                        {
                            Id = i.Id,
                            OrderId = i.OrderId,
                            ProductId = i.ProductId,
                            ProductName = i.Product != null
                                ? i.Product.Name ?? string.Empty
                                : string.Empty,
                            Quantity = i.Quantity,
                            UnitPrice = i.UnitPrice,
                            Total = i.Quantity * i.UnitPrice
                        })
                        .ToList()
                })
                .ToListAsync();
        }

        public async Task<OrderDto?> UpdateStatusAsync(
            int id,
            UpdateOrderStatusDto dto)
        {
            var order = await _db.Orders
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return null;

            order.OrderStatus = dto.OrderStatus;
            order.UpdatedDate = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return await GetByIdAsync(id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var order = await _db.Orders
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return false;

            _db.Orders.Remove(order);

            await _db.SaveChangesAsync();

            return true;
        }
    }
}