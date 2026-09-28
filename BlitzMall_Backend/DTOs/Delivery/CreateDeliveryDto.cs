using System.ComponentModel.DataAnnotations;

namespace BlitzMall_Backend.DTOs.Delivery
{
    public class CreateDeliveryDto
    {
        [Range(1, int.MaxValue)]
        public int OrderId { get; set; }

        [MaxLength(50)]
        public string? Method { get; set; }

        [MaxLength(100)]
        public string? Carrier { get; set; }

        [MaxLength(100)]
        public string? TrackingNumber { get; set; }
    }
}