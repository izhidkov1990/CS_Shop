using AuthService.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthService
{
    public class UserDbContext: DbContext
    {
        public UserDbContext(DbContextOptions<UserDbContext> options) : base(options)
        {
        }
        public DbSet<User> Users { get; set; }
    }
}
