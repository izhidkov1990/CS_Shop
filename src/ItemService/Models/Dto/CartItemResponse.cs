using System;

namespace ItemService.Models.Dto
{
    public class CartItemResponse
    {
        public Guid Id { get; set; }
        public Guid ListingId { get; set; }
        public int Quantity { get; set; }
        public DateTime CreatedAt { get; set; }
        public ListingResponse Listing { get; set; } = new ListingResponse();
    }
}
