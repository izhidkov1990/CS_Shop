using Newtonsoft.Json;
using System.Collections.Generic;

namespace ItemService.Models
{
    public class SteamDescription
    {
        [JsonProperty("appid")]
        public int AppId { get; set; }

        private string _iconUrl;
        private string _iconUrlLarge;
        private const string BaseImageUrl = "https://steamcommunity-a.akamaihd.net/economy/image";

        [JsonProperty("classid")]
        public string ClassId { get; set; }

        [JsonProperty("instanceid")]
        public string InstanceId { get; set; }

        [JsonProperty("currency")]
        public int Currency { get; set; }

        [JsonProperty("background_color")]
        public string BackgroundColor { get; set; }

        [JsonProperty("icon_url")]
        public string IconUrl
        {
            get => _iconUrl;
            set => _iconUrl = TransformToFullUrl(value);
        }

        [JsonProperty("icon_url_large")]
        public string IconUrlLarge
        {
            get => _iconUrlLarge;
            set => _iconUrlLarge = TransformToFullUrl(value);
        }

        [JsonProperty("tradable")]
        public int Tradable { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("market_name")]
        public string MarketName { get; set; }

        [JsonProperty("market_hash_name")]
        public string MarketHashName { get; set; }

        [JsonProperty("commodity")]
        public int Commodity { get; set; }

        [JsonProperty("market_tradable_restriction")]
        public int MarketTradableRestriction { get; set; }

        [JsonProperty("marketable")]
        public int Marketable { get; set; }

        [JsonProperty("descriptions")]
        public List<DescriptionDetail> Descriptions { get; set; }

        [JsonProperty("actions")]
        public List<Action> Actions { get; set; }

        [JsonProperty("market_actions")]
        public List<MarketAction> MarketActions { get; set; }

        [JsonProperty("tags")]
        public List<Tag> Tags { get; set; }

        private string TransformToFullUrl(string partialUrl)
        {
            if (!string.IsNullOrEmpty(partialUrl) && !partialUrl.StartsWith(BaseImageUrl))
            {
                return $"{BaseImageUrl}/{partialUrl}";
            }
            return partialUrl;
        }

        public class DescriptionDetail
        {
            [JsonProperty("type")]
            public string Type { get; set; }

            [JsonProperty("value")]
            public string Value { get; set; }

            [JsonProperty("color")]
            public string Color { get; set; }
        }

        public class Action
        {
            [JsonProperty("link")]
            public string Link { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }
        }

        public class MarketAction
        {
            [JsonProperty("link")]
            public string Link { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }
        }

        public class Tag
        {
            [JsonProperty("category")]
            public string Category { get; set; }

            [JsonProperty("internal_name")]
            public string InternalName { get; set; }

            [JsonProperty("localized_category_name")]
            public string LocalizedCategoryName { get; set; }

            [JsonProperty("localized_tag_name")]
            public string LocalizedTagName { get; set; }

            [JsonProperty("color")]
            public string Color { get; set; }
        }
    }
}
