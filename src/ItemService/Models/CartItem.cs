using System;

namespace ItemService.Models
{
    public class CartItem
    {
        public Guid Id { get; set; }
        public Guid ListingId { get; set; }
        public MarketListing? Listing { get; set; }
        public string BuyerSteamId { get; set; } = string.Empty;
        public int Quantity { get; set; } = 1;
        public DateTime CreatedAt { get; set; }
    }
}
