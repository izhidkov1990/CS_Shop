using ItemService;
using ItemService.Models;
using ItemService.Repositories;

public class ItemRepository : IitemRepository
{
    private readonly ItemDbContext _context;
    private readonly ILogger<ItemRepository> _logger;

    public ItemRepository(ItemDbContext context, ILogger<ItemRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task PutItemsOnSale(SteamItem[] steamItems)
    {
        await _context.AddRangeAsync(steamItems);
        await _context.SaveChangesAsync();
    }

    public async Task WithdrawFromSale()
    {
       
    }
}
