using Newtonsoft.Json;
using System.Collections.Generic;

namespace ItemService.Models
{
    public class SteamInventory
    {
        [JsonProperty("assets")]
        public List<SteamAsset> Assets { get; set; } = new();

        [JsonProperty("descriptions")]
        public List<SteamDescription> Descriptions { get; set; } = new();

        [JsonProperty("more_items")]
        public bool MoreItems { get; set; }

        [JsonProperty("last_assetid")]
        public string? LastAssetId { get; set; }

        [JsonProperty("total_inventory_count")]
        public int TotalInventoryCount { get; set; }

        [JsonProperty("success")]
        public int Success { get; set; }

        [JsonProperty("rwgrsn")]
        public int? Rwgrsn { get; set; }
    }
}
