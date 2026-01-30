using System.ComponentModel.DataAnnotations;

namespace AuthService.DTOs
{
    public class UserUpdateDTO
    {
        [EmailAddress]
        public string? Email { get; set; }

        [Phone]
        public string? Phone { get; set; }
    }
}
