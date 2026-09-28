using System.ComponentModel.DataAnnotations;

namespace BlitzMall_Backend.DTOs.Order
{
    public class UpdateOrderStatusDto
    {
        [Required, MaxLength(50)]
        public string OrderStatus { get; set; } = string.Empty;
    }
}