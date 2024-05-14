using Newtonsoft.Json;

public class SteamAsset
{
    public int AppId { get; set; }
    public string ContextId { get; set; }
    public string AssetId { get; set; }
    public string ClassId { get; set; }
    public string InstanceId { get; set; }
    public string Amount { get; set; }
    [JsonProperty("hide_in_china")]
    public int HideInChina { get; set; }
    public int Pos { get; set; }
}
