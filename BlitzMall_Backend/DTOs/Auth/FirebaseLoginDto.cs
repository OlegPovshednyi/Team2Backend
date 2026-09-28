using System.ComponentModel.DataAnnotations;

namespace BlitzMall_Backend.DTOs.Auth
{
    public class FirebaseLoginDto
    {
        [Required]
        public string IdToken { get; set; } = string.Empty;
    }
}
