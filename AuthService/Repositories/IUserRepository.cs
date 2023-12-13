using AuthService.Models;

namespace AuthService.Repositories
{
    public interface IUserRepository
    {
        Task<User?> AddUserAsync(User? user);
        Task<User?> GetUserBySteamIdAsync(string steamId);
        Task<User?> UpdateUserAsync(User user);
        Task<bool> DeleteUserAsync(int userId);
    }
}
