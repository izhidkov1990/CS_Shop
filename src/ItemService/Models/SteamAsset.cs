using Newtonsoft.Json;

namespace ItemService.Models
{
    public class SteamAsset
    {
        public int AppId { get; set; }
        public string ContextId { get; set; } = string.Empty;
        public string AssetId { get; set; } = string.Empty;
        public string ClassId { get; set; } = string.Empty;
        public string InstanceId { get; set; } = string.Empty;
        public string Amount { get; set; } = string.Empty;

        [JsonProperty("hide_in_china")]
        public int HideInChina { get; set; }

        public int Pos { get; set; }
    }
}
