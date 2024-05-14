using Newtonsoft.Json;
using System.Collections.Generic;

namespace ItemService.Models
{
    public class SteamInventory
    {
        [JsonProperty("assets")]
        public List<SteamAsset> Assets { get; set; }

        [JsonProperty("descriptions")]
        public List<SteamDescription> Descriptions { get; set; }
    }
}
