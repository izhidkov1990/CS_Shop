using System;

namespace ItemService.Models.Dto
{
    public class PurchaseResponse
    {
        public Guid Id { get; set; }
        public Guid ListingId { get; set; }
        public decimal Price { get; set; }
        public string Currency { get; set; } = "USD";
        public DateTime PurchasedAt { get; set; }
        public string SellerSteamId { get; set; } = string.Empty;
        public string BuyerSteamId { get; set; } = string.Empty;
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
