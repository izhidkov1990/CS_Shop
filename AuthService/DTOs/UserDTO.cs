using System.ComponentModel.DataAnnotations;

namespace AuthService.DTOs
{
    public class UserDTO
    {
        [Required]
        [RegularExpression(@"^\d{17}$", ErrorMessage = "Wrong Steam ID number")]
        public string SteamId { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string PersonaName { get; set; } = string.Empty;

        public string? ProfileUrl { get; set; }
        public string? Avatar { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        [Phone]
        public string? Phone { get; set; }
    }
}
