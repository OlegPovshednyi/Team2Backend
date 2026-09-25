using System.ComponentModel.DataAnnotations;

namespace BlitzMall_Backend.DTOs.Order
{
    public class CreateOrderDto
    {
        [Required] public string DeliveryAddress { get; set; } = string.Empty;
        [Required] public string Phone { get; set; } = string.Empty;
        public string? Comment { get; set; }
    }
}
