using ItemService.Models;

namespace ItemService.Services
{
    public interface IMarketplaceService
    {
        Task<MarketListing> CreateListingAsync(string sellerSteamId, int appId, string contextId, Models.Dto.CreateListingRequest request, CancellationToken cancellationToken);
        Task<MarketListing?> GetListingAsync(Guid id, CancellationToken cancellationToken);
        Task<List<MarketListing>> GetActiveListingsAsync(int skip, int take, CancellationToken cancellationToken);
        Task CancelListingAsync(Guid listingId, string sellerSteamId, CancellationToken cancellationToken);
        Task<CartItem> AddToCartAsync(string buyerSteamId, Guid listingId, CancellationToken cancellationToken);
        Task<List<CartItem>> GetCartAsync(string buyerSteamId, CancellationToken cancellationToken);
        Task RemoveFromCartAsync(string buyerSteamId, Guid listingId, CancellationToken cancellationToken);
        Task<List<Purchase>> CheckoutAsync(string buyerSteamId, CancellationToken cancellationToken);
    }
}
