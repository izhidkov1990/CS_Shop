using AuthService.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace AuthService.Repositories
{
    public class UserRepository : IUserRepository, IDisposable
    {
        private readonly UserDbContext _context;
        private bool disposed = false;

        public UserRepository(UserDbContext context)
        {
            _context = context;
        }

        public async Task<User> AddUserAsync(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

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

        public async Task<User> GetUserBySteamIdAsync(string steamId)
        {
            if (string.IsNullOrEmpty(steamId))
            {
                throw new ArgumentNullException(nameof(steamId));
            }

            return await _context.Users.FirstOrDefaultAsync(u => u.SteamID == steamId);
        }

        public async Task<User> UpdateUserAsync(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            _context.Set<User>().Update(user);
            await _context.SaveChangesAsync();
            return user;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
