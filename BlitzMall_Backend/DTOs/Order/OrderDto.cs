namespace BlitzMall_Backend.DTOs.Order
{
    public class OrderDto
    {
        public int Id { get; set; }
        public string OrderStatus { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string DeliveryAddress { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? Comment { get; set; }
        public string? PaymentMethod { get; set; }
        public DateTime? CreatedDate { get; set; }
        public List<OrderItemDto> Items { get; set; } = new();
    }
}
