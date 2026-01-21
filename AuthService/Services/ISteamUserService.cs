using AuthService.DTOs;
using AuthService.Models;

namespace AuthService.Services
{
    public interface ISteamUserService
    {
        Task<string?> AuthorizeUserAsync(string steamId);
        Task<bool> UpdateUserAsync(string steamId, UserUpdateDTO update);
        string GetSteamFromUrl(string steamUrl);
        Task<User?> GetUserBySteamId(string steamId);
    }
}
