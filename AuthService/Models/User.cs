using System.ComponentModel.DataAnnotations;

namespace AuthService.Models
{
    public class User
    {
        public User()
        {
            Id = Guid.NewGuid();
        }
        [Required]
        public Guid Id { get; set; }
        [Required]
        public string? SteamID { get; set; }
        [Required]
        public string? Name { get; set; }
        public string? AvatarURL { get; set; }
        [EmailAddress]
        public string? Email { get; set; }
        [Phone]
        public string? Phone { get; set; }
        [Required]
        public DateTime? DateOfAuth { get; set; }
    }
}
