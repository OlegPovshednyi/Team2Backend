namespace BlitzMall_Backend.DTOs.Delivery
{
    public class DeliveryDto
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string? Method { get; set; }
        public string? Carrier { get; set; }
        public string? TrackingNumber { get; set; }
        public string? Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ShippedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
    }
}