using ItemService.Models;

namespace ItemService.Services
{
    public interface ISteamItemService
    {
        Task<List<SteamItem>> GetItemsFromSteamAPI(string steamId, string appid, string contextid);

        Task ClearCacheAsync(string steamId, string appid, string contextid);
    }
}
