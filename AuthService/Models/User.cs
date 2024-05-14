using System.ComponentModel.DataAnnotations;

namespace AuthService.Models
{
    public class User
    {
        public Guid Id { get; private set; } = Guid.NewGuid();

        [Required]
        [RegularExpression(@"^\d{17}$", ErrorMessage = "Wrong Steam ID number")]
        public string SteamID { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Name { get; set; }

        public string AvatarURL { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        [Phone]
        public string? Phone { get; set; }

        public DateTime DateOfAuth { get; set; } = DateTime.UtcNow;
    }
}
