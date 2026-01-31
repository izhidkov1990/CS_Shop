using System.Collections.Generic;

namespace ItemService.Models.Dto
{
    public class CheckoutResponse
    {
        public decimal Total { get; set; }
        public string Currency { get; set; } = "USD";
        public List<PurchaseResponse> Purchases { get; set; } = new List<PurchaseResponse>();
    }
}
