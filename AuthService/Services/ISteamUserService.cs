using AuthService.DTOs;
using AuthService.Models;

namespace AuthService.Services
{
    public interface ISteamUserService
    {
        Task<string>AuthorizeUserAsync(string steamId);
        Task<bool> UpdateUserAsync(User user);
        string GetSteamFromUrl(string steamUrl);
        Task<User> GetUserBySteamId(string steamId);
    }
}
