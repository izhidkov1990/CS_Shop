namespace ItemService.Models
{
    public class SteamItem
    {
        public int AppId { get; set; }
        public string ContextId { get; set; }
        public string AssetId { get; set; }
        public string ClassId { get; set; }
        public string InstanceId { get; set; }
        public string Amount { get; set; }
        public SteamDescription Description { get; set; }
    }

}
