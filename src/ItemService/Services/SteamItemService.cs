using ItemService.Exceptions;
using ItemService.Models;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ItemService.Services
{
    public class SteamItemService : ISteamItemService
    {
        private const int PageSize = 5000;
        private const int MaxPages = 20;
        private readonly IDistributedCache _distributedCache;
        private readonly IHttpClientFactory _httpClientFactory;

        public SteamItemService(IDistributedCache distributedCache, IHttpClientFactory httpClientFactory)
        {
            _distributedCache = distributedCache;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<List<SteamItem>> GetItemsFromSteamAPI(string steamId, string appid, string contextid, CancellationToken cancellationToken = default)
        {
            string cacheKey = $"SteamItems_{steamId}_{appid}_{contextid}";

            var cachedData = await _distributedCache.GetStringAsync(cacheKey, cancellationToken);
            if (cachedData == null)
            {
                SteamInventory steamInventory = await FetchInventoryAsync(steamId, appid, contextid, cancellationToken);
                var serializedData = JsonConvert.SerializeObject(steamInventory);
                await _distributedCache.SetStringAsync(cacheKey, serializedData, new DistributedCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromMinutes(120)
                }, cancellationToken);

                return CreateSteamItemsList(steamInventory);
            }

            var cachedInventory = JsonConvert.DeserializeObject<SteamInventory>(cachedData) ?? new SteamInventory();
            return CreateSteamItemsList(cachedInventory);
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

        private async Task<SteamInventory> FetchInventoryAsync(string steamId, string appid, string contextid, CancellationToken cancellationToken)
        {
            var allAssets = new List<SteamAsset>();
            var descriptionMap = new Dictionary<string, SteamDescription>(StringComparer.Ordinal);
            string? startAssetId = null;
            bool moreItems = true;
            int pageCount = 0;

            while (moreItems)
            {
                var page = await FetchInventoryPageAsync(steamId, appid, contextid, startAssetId, cancellationToken);
                if (page.Assets != null && page.Assets.Count > 0)
                {
                    allAssets.AddRange(page.Assets);
                }

                if (page.Descriptions != null && page.Descriptions.Count > 0)
                {
                    foreach (var description in page.Descriptions)
                    {
                        var key = $"{description.ClassId}:{description.InstanceId}";
                        if (!descriptionMap.ContainsKey(key))
                        {
                            descriptionMap[key] = description;
                        }
                    }
                }

                moreItems = page.MoreItems;
                if (!moreItems)
                {
                    break;
                }

                if (string.IsNullOrWhiteSpace(page.LastAssetId) || page.LastAssetId == startAssetId)
                {
                    break;
                }

                startAssetId = page.LastAssetId;
                pageCount++;
                if (pageCount >= MaxPages)
                {
                    break;
                }
            }

            return new SteamInventory
            {
                Assets = allAssets,
                Descriptions = descriptionMap.Values.ToList(),
                MoreItems = moreItems,
                LastAssetId = startAssetId,
                TotalInventoryCount = allAssets.Count,
                Success = 1
            };
        }

        private async Task<SteamInventory> FetchInventoryPageAsync(string steamId, string appid, string contextid, string? startAssetId, CancellationToken cancellationToken)
        {
            var url = $"https://steamcommunity.com/inventory/{steamId}/{appid}/{contextid}?l=russian&count={PageSize}";
            if (!string.IsNullOrWhiteSpace(startAssetId))
            {
                url = $"{url}&start_assetid={startAssetId}";
            }

            using var httpClient = _httpClientFactory.CreateClient("SteamInventory");
            using var response = await httpClient.GetAsync(url, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                throw new SteamApiException($"Ошибка запроса при получении коллекции предметов Steam API: {response.StatusCode}", response.StatusCode);
            }

            var result = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonConvert.DeserializeObject<SteamInventory>(result) ?? new SteamInventory();
        }

        public async Task ClearCacheAsync(string steamId, string appid, string contextid)
        {
            string cacheKey = $"SteamItems_{steamId}_{appid}_{contextid}";
            await _distributedCache.RemoveAsync(cacheKey);
        }
    }
}
