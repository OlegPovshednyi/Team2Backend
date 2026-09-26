namespace BlitzMall_Backend.DTOs.User
{
    public class ProfileDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public DateTime? BirthDate { get; set; }
        public DateTime CreatedAt { get; set; }

        public int OrdersCount { get; set; }
        public decimal Bonuses { get; set; }
        public string Level { get; set; } = "Бронзовий";
        public int LevelProgressPercent { get; set; }
        public string NextLevelHint { get; set; } = string.Empty;
    }

    public class UpdateProfileDto
    {
        public string? Name { get; set; }
        public string? Phone { get; set; }
        public DateTime? BirthDate { get; set; }
    }
}
