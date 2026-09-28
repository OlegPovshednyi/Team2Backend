using System.ComponentModel.DataAnnotations;

namespace BlitzMall_Backend.DTOs.Auth
{
    public class GoogleLoginDto
    {
        [Required]
        public string IdToken { get; set; } = string.Empty;
    }
}