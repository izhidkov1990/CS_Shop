using ItemService.Models;
using Microsoft.EntityFrameworkCore;

namespace ItemService
{
    public class ItemDbContext: DbContext
    {
        public ItemDbContext(DbContextOptions<ItemDbContext> options) : base(options)
        {
        }

        public DbSet<SteamItem> Items { get; set; }
    }
}
