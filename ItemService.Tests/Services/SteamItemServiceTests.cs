using ItemService.Exceptions;
using ItemService.Models;
using ItemService.Services;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Net;
using System.Text;

namespace ItemService.Tests.Services
{
    public class SteamItemServiceTests
    {
        private const string SteamId = "76561198000000000";
        private const string AppId = "730";
        private const string ContextId = "2";

        [Test]
        public async Task GetItemsFromSteamAPI_UsesCache_WhenAvailable()
        {
            var cache = new TestDistributedCache();
            var inventory = BuildInventory();
            var cacheKey = BuildCacheKey();
            await cache.SetAsync(cacheKey, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(inventory)), new DistributedCacheEntryOptions());

            var handler = new ThrowingHttpMessageHandler();
            var service = CreateService(cache, handler);

            var items = await service.GetItemsFromSteamAPI(SteamId, AppId, ContextId);

            Assert.AreEqual(1, items.Count);
            Assert.AreEqual("AK-47", items[0].Description.Name);
        }

        [Test]
        public async Task GetItemsFromSteamAPI_CachesResponse_OnCacheMiss()
        {
            var cache = new TestDistributedCache();
            var handler = new OkHttpMessageHandler(BuildInventoryJson());
            var service = CreateService(cache, handler);

            var items = await service.GetItemsFromSteamAPI(SteamId, AppId, ContextId);

            Assert.AreEqual(1, items.Count);
            Assert.IsTrue(cache.Contains(BuildCacheKey()));
        }

        [Test]
        public void GetItemsFromSteamAPI_ThrowsSteamApiException_OnFailure()
        {
            var cache = new TestDistributedCache();
            var handler = new StatusCodeHttpMessageHandler(HttpStatusCode.BadGateway);
            var service = CreateService(cache, handler);

            var ex = Assert.ThrowsAsync<SteamApiException>(() => service.GetItemsFromSteamAPI(SteamId, AppId, ContextId));

            Assert.AreEqual(HttpStatusCode.BadGateway, ex?.StatusCode);
        }

        [Test]
        public async Task ClearCacheAsync_RemovesCacheEntry()
        {
            var cache = new TestDistributedCache();
            var cacheKey = BuildCacheKey();
            await cache.SetAsync(cacheKey, Encoding.UTF8.GetBytes("cached"), new DistributedCacheEntryOptions());
            var service = CreateService(cache, new OkHttpMessageHandler(BuildInventoryJson()));

            await service.ClearCacheAsync(SteamId, AppId, ContextId);

            Assert.IsFalse(cache.Contains(cacheKey));
        }

        private static SteamItemService CreateService(TestDistributedCache cache, HttpMessageHandler handler)
        {
            var httpClient = new HttpClient(handler);
            var factory = new TestHttpClientFactory(httpClient);
            return new SteamItemService(cache, factory);
        }

        private static string BuildCacheKey() => $"SteamItems_{SteamId}_{AppId}_{ContextId}";

        private static SteamInventory BuildInventory()
        {
            return new SteamInventory
            {
                Assets = new List<SteamAsset>
                {
                    new SteamAsset
                    {
                        AppId = 730,
                        ContextId = "2",
                        AssetId = "1",
                        ClassId = "100",
                        InstanceId = "0",
                        Amount = "1"
                    }
                },
                Descriptions = new List<SteamDescription>
                {
                    new SteamDescription
                    {
                        AppId = 730,
                        ClassId = "100",
                        InstanceId = "0",
                        Name = "AK-47",
                        Type = "Weapon",
                        MarketName = "AK-47",
                        MarketHashName = "AK-47",
                        Tradable = 1,
                        Marketable = 1,
                        Commodity = 0,
                        Currency = 0,
                        BackgroundColor = "",
                        Descriptions = new List<SteamDescription.DescriptionDetail>(),
                        Actions = new List<SteamDescription.Action>(),
                        MarketActions = new List<SteamDescription.MarketAction>(),
                        Tags = new List<SteamDescription.Tag>(),
                        IconUrl = "icon",
                        IconUrlLarge = "icon_large"
                    }
                }
            };
        }

        private static string BuildInventoryJson()
        {
            return JsonConvert.SerializeObject(BuildInventory());
        }

        private sealed class TestHttpClientFactory : IHttpClientFactory
        {
            private readonly HttpClient _client;

            public TestHttpClientFactory(HttpClient client)
            {
                _client = client;
            }

            public HttpClient CreateClient(string name) => _client;
        }

        private sealed class TestDistributedCache : IDistributedCache
        {
            private readonly ConcurrentDictionary<string, byte[]> _store = new();

            public byte[]? Get(string key)
            {
                _store.TryGetValue(key, out var value);
                return value;
            }

            public Task<byte[]?> GetAsync(string key, CancellationToken token = default)
            {
                return Task.FromResult(Get(key));
            }

            public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
            {
                _store[key] = value;
            }

            public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
            {
                Set(key, value, options);
                return Task.CompletedTask;
            }

            public void Refresh(string key)
            {
            }

            public Task RefreshAsync(string key, CancellationToken token = default)
            {
                return Task.CompletedTask;
            }

            public void Remove(string key)
            {
                _store.TryRemove(key, out _);
            }

            public Task RemoveAsync(string key, CancellationToken token = default)
            {
                Remove(key);
                return Task.CompletedTask;
            }

            public bool Contains(string key) => _store.ContainsKey(key);
        }

        private sealed class OkHttpMessageHandler : HttpMessageHandler
        {
            private readonly string _payload;

            public OkHttpMessageHandler(string payload)
            {
                _payload = payload;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(_payload, Encoding.UTF8, "application/json")
                };

                return Task.FromResult(response);
            }
        }

        private sealed class StatusCodeHttpMessageHandler : HttpMessageHandler
        {
            private readonly HttpStatusCode _statusCode;

            public StatusCodeHttpMessageHandler(HttpStatusCode statusCode)
            {
                _statusCode = statusCode;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return Task.FromResult(new HttpResponseMessage(_statusCode));
            }
        }

        private sealed class ThrowingHttpMessageHandler : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                throw new InvalidOperationException("HTTP client should not be called when cache hit.");
            }
        }
    }
}
