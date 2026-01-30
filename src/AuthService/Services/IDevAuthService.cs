using AuthService.DTOs;

namespace AuthService.Services
{
    public interface IDevAuthService
    {
        Task<string?> AuthorizeDevUserAsync(DevLoginRequest request);
    }
}
