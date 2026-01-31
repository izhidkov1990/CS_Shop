using ItemService.Models;
using Microsoft.EntityFrameworkCore;

namespace ItemService.Data
{
    public class MarketDbContext : DbContext
    {
        public MarketDbContext(DbContextOptions<MarketDbContext> options) : base(options)
        {
        }

        public DbSet<MarketListing> MarketListings => Set<MarketListing>();
        public DbSet<CartItem> CartItems => Set<CartItem>();
        public DbSet<Purchase> Purchases => Set<Purchase>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MarketListing>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Price).HasPrecision(18, 2);
                entity.Property(e => e.Currency).HasMaxLength(8);
                entity.Property(e => e.SellerSteamId).HasMaxLength(64);
                entity.Property(e => e.BuyerSteamId).HasMaxLength(64);
                entity.Property(e => e.AssetId).HasMaxLength(64);
                entity.Property(e => e.ClassId).HasMaxLength(64);
                entity.Property(e => e.InstanceId).HasMaxLength(64);
                entity.Property(e => e.ContextId).HasMaxLength(32);
                entity.Property(e => e.Name).HasMaxLength(200);
                entity.Property(e => e.MarketHashName).HasMaxLength(200);
                entity.Property(e => e.ItemType).HasMaxLength(200);
                entity.Property(e => e.IconUrl).HasMaxLength(400);
                entity.Property(e => e.RowVersion).IsRowVersion();
                entity.HasIndex(e => new { e.Status, e.CreatedAt });
                entity.HasIndex(e => new { e.SellerSteamId, e.Status });
                entity.HasIndex(e => new { e.SellerSteamId, e.AssetId, e.Status });
            });

            modelBuilder.Entity<CartItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.BuyerSteamId).HasMaxLength(64);
                entity.HasIndex(e => new { e.BuyerSteamId, e.ListingId }).IsUnique();
                entity.HasOne(e => e.Listing)
                    .WithMany(l => l.CartItems)
                    .HasForeignKey(e => e.ListingId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Purchase>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Price).HasPrecision(18, 2);
                entity.Property(e => e.Currency).HasMaxLength(8);
                entity.Property(e => e.BuyerSteamId).HasMaxLength(64);
                entity.Property(e => e.SellerSteamId).HasMaxLength(64);
                entity.Property(e => e.ContextId).HasMaxLength(32);
                entity.Property(e => e.AssetId).HasMaxLength(64);
                entity.Property(e => e.ClassId).HasMaxLength(64);
                entity.Property(e => e.InstanceId).HasMaxLength(64);
                entity.Property(e => e.ItemName).HasMaxLength(200);
                entity.Property(e => e.MarketHashName).HasMaxLength(200);
                entity.Property(e => e.ItemType).HasMaxLength(200);
                entity.Property(e => e.IconUrl).HasMaxLength(400);
                entity.HasIndex(e => new { e.BuyerSteamId, e.PurchasedAt });
                entity.HasOne(e => e.Listing)
                    .WithMany(l => l.Purchases)
                    .HasForeignKey(e => e.ListingId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
