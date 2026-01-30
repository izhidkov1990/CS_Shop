using System.ComponentModel.DataAnnotations;

namespace AuthService.DTOs
{
    public class DevLoginRequest
    {
        [Required]
        [RegularExpression(@"^\d{17}$", ErrorMessage = "Wrong Steam ID number")]
        public string SteamId { get; set; } = string.Empty;

        [StringLength(100, MinimumLength = 3)]
        public string? Name { get; set; }

        public string? AvatarUrl { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        [Phone]
        public string? Phone { get; set; }
    }
}
