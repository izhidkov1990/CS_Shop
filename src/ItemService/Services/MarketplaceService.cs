using ItemService.Data;
using ItemService.Exceptions;
using ItemService.Models;
using ItemService.Models.Dto;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ItemService.Services
{
    public class MarketplaceService : IMarketplaceService
    {
        private readonly MarketDbContext _dbContext;
        private readonly ISteamItemService _steamItemService;

        public MarketplaceService(MarketDbContext dbContext, ISteamItemService steamItemService)
        {
            _dbContext = dbContext;
            _steamItemService = steamItemService;
        }

        public async Task<MarketListing> CreateListingAsync(string sellerSteamId, int appId, string contextId, CreateListingRequest request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(sellerSteamId))
            {
                throw new MarketplaceException("Seller is required.");
            }

            if (string.IsNullOrWhiteSpace(request.AssetId))
            {
                throw new MarketplaceException("assetId is required.");
            }

            if (request.Price <= 0)
            {
                throw new MarketplaceException("Price must be greater than 0.");
            }

            var existingListing = await _dbContext.MarketListings
                .AsNoTracking()
                .AnyAsync(l => l.SellerSteamId == sellerSteamId
                    && l.AssetId == request.AssetId
                    && l.Status == ListingStatus.Active, cancellationToken);
            if (existingListing)
            {
                throw new MarketplaceException("This item is already listed.", 409);
            }

            SteamItem? steamItem = null;
            if (request.ValidateOwnership)
            {
                var items = await _steamItemService.GetItemsFromSteamAPI(
                    sellerSteamId,
                    appId.ToString(),
                    contextId,
                    cancellationToken);
                steamItem = items.FirstOrDefault(i => i.AssetId == request.AssetId);
                if (steamItem == null)
                {
                    throw new MarketplaceException("Item not found in your Steam inventory.", 404);
                }
            }

            var listing = new MarketListing
            {
                Id = Guid.NewGuid(),
                SellerSteamId = sellerSteamId,
                AppId = appId,
                ContextId = contextId,
                AssetId = request.AssetId,
                ClassId = request.ClassId ?? steamItem?.ClassId ?? string.Empty,
                InstanceId = request.InstanceId ?? steamItem?.InstanceId ?? string.Empty,
                Amount = request.Amount ?? ParseAmount(steamItem?.Amount) ?? 1,
                Name = request.Name ?? steamItem?.Description?.Name ?? string.Empty,
                MarketHashName = request.MarketHashName ?? steamItem?.Description?.MarketHashName ?? string.Empty,
                ItemType = request.ItemType ?? steamItem?.Description?.Type ?? string.Empty,
                IconUrl = request.IconUrl ?? steamItem?.Description?.IconUrl ?? string.Empty,
                Price = request.Price,
                Currency = string.IsNullOrWhiteSpace(request.Currency) ? "USD" : request.Currency,
                Status = ListingStatus.Active,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.MarketListings.Add(listing);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return listing;
        }

        public async Task<MarketListing?> GetListingAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _dbContext.MarketListings
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.Id == id, cancellationToken);
        }

        public async Task<List<MarketListing>> GetActiveListingsAsync(int skip, int take, CancellationToken cancellationToken)
        {
            return await _dbContext.MarketListings
                .AsNoTracking()
                .Where(l => l.Status == ListingStatus.Active)
                .OrderByDescending(l => l.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync(cancellationToken);
        }

        public async Task CancelListingAsync(Guid listingId, string sellerSteamId, CancellationToken cancellationToken)
        {
            var listing = await _dbContext.MarketListings
                .FirstOrDefaultAsync(l => l.Id == listingId, cancellationToken);
            if (listing == null)
            {
                throw new MarketplaceException("Listing not found.", 404);
            }

            if (!string.Equals(listing.SellerSteamId, sellerSteamId, StringComparison.Ordinal))
            {
                throw new MarketplaceException("You can cancel only your own listings.", 403);
            }

            if (listing.Status != ListingStatus.Active)
            {
                throw new MarketplaceException("Listing is not active.", 409);
            }

            listing.Status = ListingStatus.Cancelled;
            listing.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<CartItem> AddToCartAsync(string buyerSteamId, Guid listingId, CancellationToken cancellationToken)
        {
            var listing = await _dbContext.MarketListings
                .FirstOrDefaultAsync(l => l.Id == listingId, cancellationToken);
            if (listing == null)
            {
                throw new MarketplaceException("Listing not found.", 404);
            }

            if (listing.Status != ListingStatus.Active)
            {
                throw new MarketplaceException("Listing is not available.", 409);
            }

            if (string.Equals(listing.SellerSteamId, buyerSteamId, StringComparison.Ordinal))
            {
                throw new MarketplaceException("You cannot buy your own listing.");
            }

            var existing = await _dbContext.CartItems
                .Include(c => c.Listing)
                .FirstOrDefaultAsync(c => c.BuyerSteamId == buyerSteamId && c.ListingId == listingId, cancellationToken);
            if (existing != null)
            {
                return existing;
            }

            var cartItem = new CartItem
            {
                Id = Guid.NewGuid(),
                BuyerSteamId = buyerSteamId,
                ListingId = listingId,
                CreatedAt = DateTime.UtcNow,
                Quantity = 1,
                Listing = listing
            };

            _dbContext.CartItems.Add(cartItem);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return cartItem;
        }

        public async Task<List<CartItem>> GetCartAsync(string buyerSteamId, CancellationToken cancellationToken)
        {
            return await _dbContext.CartItems
                .AsNoTracking()
                .Include(c => c.Listing)
                .Where(c => c.BuyerSteamId == buyerSteamId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task RemoveFromCartAsync(string buyerSteamId, Guid listingId, CancellationToken cancellationToken)
        {
            var cartItem = await _dbContext.CartItems
                .FirstOrDefaultAsync(c => c.BuyerSteamId == buyerSteamId && c.ListingId == listingId, cancellationToken);
            if (cartItem == null)
            {
                return;
            }

            _dbContext.CartItems.Remove(cartItem);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<List<Purchase>> CheckoutAsync(string buyerSteamId, CancellationToken cancellationToken)
        {
            var cartItems = await _dbContext.CartItems
                .Include(c => c.Listing)
                .Where(c => c.BuyerSteamId == buyerSteamId)
                .ToListAsync(cancellationToken);
            if (cartItems.Count == 0)
            {
                throw new MarketplaceException("Cart is empty.");
            }

            var purchases = new List<Purchase>();

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
            try
            {
                foreach (var cartItem in cartItems)
                {
                    var listing = await _dbContext.MarketListings
                        .FirstOrDefaultAsync(l => l.Id == cartItem.ListingId, cancellationToken);
                    if (listing == null)
                    {
                        throw new MarketplaceException("Listing not found.", 404);
                    }

                    if (listing.Status != ListingStatus.Active)
                    {
                        throw new MarketplaceException("Some listings are no longer available.", 409);
                    }

                    if (string.Equals(listing.SellerSteamId, buyerSteamId, StringComparison.Ordinal))
                    {
                        throw new MarketplaceException("You cannot buy your own listing.");
                    }

                    listing.Status = ListingStatus.Sold;
                    listing.BuyerSteamId = buyerSteamId;
                    listing.SoldAt = DateTime.UtcNow;
                    listing.UpdatedAt = DateTime.UtcNow;

                    var purchase = new Purchase
                    {
                        Id = Guid.NewGuid(),
                        ListingId = listing.Id,
                        BuyerSteamId = buyerSteamId,
                        SellerSteamId = listing.SellerSteamId,
                        Price = listing.Price,
                        Currency = listing.Currency,
                        PurchasedAt = DateTime.UtcNow,
                        ItemName = listing.Name,
                        MarketHashName = listing.MarketHashName,
                        ItemType = listing.ItemType,
                        IconUrl = listing.IconUrl,
                        AppId = listing.AppId,
                        ContextId = listing.ContextId,
                        AssetId = listing.AssetId,
                        ClassId = listing.ClassId,
                        InstanceId = listing.InstanceId,
                        Amount = listing.Amount
                    };

                    purchases.Add(purchase);
                    _dbContext.Purchases.Add(purchase);
                    _dbContext.CartItems.Remove(cartItem);
                }

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                await transaction.RollbackAsync(cancellationToken);
                throw new MarketplaceException("Some listings were already sold.", 409);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }

            return purchases;
        }

        private static int? ParseAmount(string? amount)
        {
            if (int.TryParse(amount, out var value))
            {
                return value;
            }

            return null;
        }
    }
}
