using System.ComponentModel.DataAnnotations;

namespace AuthService
{
    public class SteamSettings
    {
        [Required]
        public string ApiKey { get; set; } = string.Empty;

        public string? SteamId { get; set; }
    }

    public class JwtSettings
    {
        [Required]
        public string SecretKey { get; set; } = string.Empty;

        [Required]
        public string Issuer { get; set; } = string.Empty;

        [Required]
        public string Audience { get; set; } = string.Empty;

        [Range(1, 365)]
        public int ExpireDays { get; set; } = 5;
    }

    public class DevAuthSettings
    {
        public bool Enabled { get; set; }

        public string? SharedKey { get; set; }
    }
}
