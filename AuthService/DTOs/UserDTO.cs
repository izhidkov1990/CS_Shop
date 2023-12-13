using System.ComponentModel.DataAnnotations;

namespace AuthService.DTOs
{
    public class UserDTO
    {
        public string? SteamId { get; set; }
        public string? PersonaName { get; set; }
        public string? ProfileUrl { get; set; }
        public string? Avatar { get; set; }

    }
}
