namespace ItemService.Models.Dto
{
    public class CreateListingRequest
    {
        public int? AppId { get; set; }
        public string? ContextId { get; set; }
        public string AssetId { get; set; } = string.Empty;
        public string? ClassId { get; set; }
        public string? InstanceId { get; set; }
        public int? Amount { get; set; }
        public decimal Price { get; set; }
        public string Currency { get; set; } = "USD";
        public bool ValidateOwnership { get; set; } = true;
        public string? Name { get; set; }
        public string? MarketHashName { get; set; }
        public string? ItemType { get; set; }
        public string? IconUrl { get; set; }
    }
}
