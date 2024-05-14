using ItemService.Models;

namespace ItemService.Repositories
{
    public interface IitemRepository
    {
        Task PutItemsOnSale(SteamItem[] steamItems);
        Task WithdrawFromSale();
    }
}
