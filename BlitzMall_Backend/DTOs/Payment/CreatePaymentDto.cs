using System.ComponentModel.DataAnnotations;

namespace BlitzMall_Backend.DTOs.Payment
{
    public class CreatePaymentDto
    {
        [Range(1, int.MaxValue)]
        public int OrderId { get; set; }

        [Required, MaxLength(50)]
        public string Method { get; set; } = string.Empty;
    }
}