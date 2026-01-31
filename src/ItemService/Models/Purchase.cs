using System;

namespace ItemService.Models
{
    public class Purchase
    {
        public Guid Id { get; set; }
        public Guid ListingId { get; set; }
        public MarketListing? Listing { get; set; }
        public string BuyerSteamId { get; set; } = string.Empty;
        public string SellerSteamId { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Currency { get; set; } = "USD";
        public DateTime PurchasedAt { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string MarketHashName { get; set; } = string.Empty;
        public string ItemType { get; set; } = string.Empty;
        public string IconUrl { get; set; } = string.Empty;
        public int AppId { get; set; }
        public string ContextId { get; set; } = string.Empty;
        public string AssetId { get; set; } = string.Empty;
        public string ClassId { get; set; } = string.Empty;
        public string InstanceId { get; set; } = string.Empty;
        public int Amount { get; set; }
    }
}
