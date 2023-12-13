using AuthService.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly UserDbContext _context;
        public UserRepository(UserDbContext context)
        {
            _context = context;
        }
        public async Task<User?> AddUserAsync(User user)
        {
            var existingUser = await _context.Users
                                     .FirstOrDefaultAsync(u => u.SteamID == user.SteamID);

            if (existingUser != null)
            {
                return null;
            }

            await _context.AddAsync(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<bool> DeleteUserAsync(int userId)
        {
            var user = await _context.Set<User>().FindAsync(userId);
            if (user == null)
            {
                return false;
            }

            _context.Set<User>().Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<User?> GetUserBySteamIdAsync(string steamId)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.SteamID == steamId);
        }

        public async Task<User?> UpdateUserAsync(User user)
        {
            _context.Set<User>().Update(user);
            await _context.SaveChangesAsync();
            return user;
        }
    }
}
