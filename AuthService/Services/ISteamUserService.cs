using AuthService.DTOs;
using AuthService.Models;

namespace AuthService.Services
{
    public interface ISteamUserService
    {
        public Task<string>AuthorizeUserAsync(string steamId);
    }
}
