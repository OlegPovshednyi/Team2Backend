using System.ComponentModel.DataAnnotations;

namespace BlitzMall_Backend.DTOs.Payment
{
    public class UpdatePaymentDto
    {
        [Required, MaxLength(50)]
        public string Status { get; set; } = string.Empty;

        public DateTime? CompletedAt { get; set; }
    }
}