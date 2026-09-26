using System.ComponentModel.DataAnnotations;



      namespace BlitzMall_Backend.Models
    {
        public class User
        {
        public int Id { get; set; }

        [MaxLength(100)]
        public string? Name { get; set; }

        [MaxLength(50)]
        public string? Status { get; set; }

        [MaxLength(254)]
        [Required]
        public string Email { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? Phone { get; set; }

        public DateTime? BirthDate { get; set; }

        [MaxLength(255)]
        public string? PasswordHash { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public Cart? Cart { get; set; }
        public Seller? Seller { get; set; }

        public int RoleId { get; set; }
        public Role? Role { get; set; }
    }
    }



