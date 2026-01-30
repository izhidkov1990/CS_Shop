using ItemService.Exceptions;
using ItemService.Models;
using ItemService.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ItemService.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class SteamItemController : ControllerBase
    {
        private const int AppId = 730;
        private const int ContextId = 2;

        private readonly ISteamItemService _steamItemService;
        private readonly ILogger<SteamItemController> _logger;

        public SteamItemController(ISteamItemService steamItemService, ILogger<SteamItemController> logger)
        {
            _steamItemService = steamItemService;
            _logger = logger;
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet]
        [Route("GetSteamItems")]
        public async Task<ActionResult<IEnumerable<SteamItem>>> GetAllSteamItems([FromQuery] string steamId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(steamId))
                {
                    return BadRequest("steamId is required.");
                }

                var steamItems = await _steamItemService.GetItemsFromSteamAPI(steamId, AppId.ToString(), ContextId.ToString());
                return Ok(steamItems);
            }
            catch (SteamApiException steamApiEx)
            {
                _logger.LogError(steamApiEx, "Ошибка при запросе к Steam API: {StatusCode}", steamApiEx.StatusCode);
                return StatusCode((int)steamApiEx.StatusCode, steamApiEx.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Неизвестная ошибка при получении предметов Steam");
                return BadRequest("Произошла ошибка при обработке вашего запроса");
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Route("ClearCache")]
        public async Task<IActionResult> ClearCache([FromQuery] string steamId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(steamId))
                {
                    return BadRequest("steamId is required.");
                }

                await _steamItemService.ClearCacheAsync(steamId, AppId.ToString(), ContextId.ToString());
                return Ok("Cache successfully cleared");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при очистке кэша");
                return BadRequest("Ошибка при очистке кэша");
            }
        }
    }
}
