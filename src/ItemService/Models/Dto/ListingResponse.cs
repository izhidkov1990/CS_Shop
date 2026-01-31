using System;

namespace ItemService.Models.Dto
{
    public class ListingResponse
    {
        public Guid Id { get; set; }
        public string SellerSteamId { get; set; } = string.Empty;
        public int AppId { get; set; }
        public string ContextId { get; set; } = string.Empty;
        public string AssetId { get; set; } = string.Empty;
        public string ClassId { get; set; } = string.Empty;
        public string InstanceId { get; set; } = string.Empty;
        public int Amount { get; set; }
        public string Name { get; set; } = string.Empty;
        public string MarketHashName { get; set; } = string.Empty;
        public string ItemType { get; set; } = string.Empty;
        public string IconUrl { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Currency { get; set; } = "USD";
        public ListingStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? SoldAt { get; set; }
    }
}
