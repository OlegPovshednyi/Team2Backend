using System.ComponentModel.DataAnnotations;

namespace BlitzMall_Backend.DTOs.Auth
{
    public class VerifyCodeDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Code { get; set; } = string.Empty;
    }
}
