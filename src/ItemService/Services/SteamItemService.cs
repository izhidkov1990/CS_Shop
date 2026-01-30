using ItemService.Exceptions;
using ItemService.Models;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System;

namespace ItemService.Services
{
    public class SteamItemService: ISteamItemService
    {
        private readonly IDistributedCache _distributedCache;
        private readonly IHttpClientFactory _httpClientFactory;
        public SteamItemService(IDistributedCache distributedCache, IHttpClientFactory httpClientFactory)
        {
            _distributedCache = distributedCache;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<List<SteamItem>> GetItemsFromSteamAPI(string steamId, string appid, string contextid)
        {
            SteamInventory steamInventory;
            string cacheKey = $"SteamItems_{steamId}_{appid}_{contextid}";

            //var url = $"https://steamcommunity.com/profiles/{steamId}/inventory/json/{appid}/{contextid}?l=english&count=5000";
            var url = $"https://steamcommunity.com/inventory/{steamId}/{appid}/{contextid}?l=russian&count=5000";
        
            var cachedData = await _distributedCache.GetStringAsync(cacheKey);
            if (cachedData == null)
            {
                using var httpClient = _httpClientFactory.CreateClient();
                using var response = await httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    throw new SteamApiException($"Ошибка запроса при получении коллекции предметов Steam API: {response.StatusCode}", response.StatusCode);
                }
                using var content = response.Content;
                var result = await content.ReadAsStringAsync();
                steamInventory = JsonConvert.DeserializeObject<SteamInventory>(result) ?? new SteamInventory();
                var serializedData = JsonConvert.SerializeObject(steamInventory);
                await _distributedCache.SetStringAsync(cacheKey, serializedData, new DistributedCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromMinutes(120)
                });

            }
            else
            {
                steamInventory = JsonConvert.DeserializeObject<SteamInventory>(cachedData) ?? new SteamInventory();
            }
            var steamItems = CreateSteamItemsList(steamInventory);
            return steamItems;
        }

        private List<SteamItem> CreateSteamItemsList(SteamInventory inventory)
        {
            var itemsList = new List<SteamItem>();

            foreach (var asset in inventory.Assets)
            {
                var description = inventory.Descriptions
                    .FirstOrDefault(d => d.ClassId == asset.ClassId && d.InstanceId == asset.InstanceId);

                if (description != null)
                {
                    var item = new SteamItem
                    {
                        AppId = asset.AppId,
                        ContextId = asset.ContextId,
                        AssetId = asset.AssetId,
                        ClassId = asset.ClassId,
                        InstanceId = asset.InstanceId,
                        Amount = asset.Amount,
                        Description = description
                    };

                    itemsList.Add(item);
                }
            }

            return itemsList;
        }

        public async Task ClearCacheAsync(string steamId, string appid, string contextid)
        {
            string cacheKey = $"SteamItems_{steamId}_{appid}_{contextid}";
            await _distributedCache.RemoveAsync(cacheKey);
        }

    }
}
