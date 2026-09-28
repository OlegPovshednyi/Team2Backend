using System.ComponentModel.DataAnnotations;

namespace BlitzMall_Backend.DTOs.Delivery
{
    public class UpdateDeliveryDto
    {
        [MaxLength(50)]
        public string? Status { get; set; }

        [MaxLength(100)]
        public string? Carrier { get; set; }

        [MaxLength(100)]
        public string? TrackingNumber { get; set; }

        public DateTime? ShippedAt { get; set; }

        public DateTime? DeliveredAt { get; set; }
    }
}