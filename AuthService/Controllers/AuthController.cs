using AuthService.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace AuthService.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ISteamUserService _authService;

        public AuthController(ISteamUserService authService)
        {
            _authService = authService;
        }

        [HttpGet("callback")]
        public async Task<IActionResult> CallbackAsync()
        {
            var claimsPrincipal = HttpContext.User;
            var steamIdClaim = claimsPrincipal.FindFirst(claim => claim.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
            var steamIdUrl = steamIdClaim?.Value;
            if(steamIdUrl != null)
            {
                string steamId = GetSteamFromUrl(steamIdUrl);
                var jwtToken = await _authService.AuthorizeUserAsync(steamId);
                if (!string.IsNullOrEmpty(jwtToken))
                {
                    return Ok(new { token = jwtToken });
                }
                else
                {
                    return Unauthorized();
                }
            }
            else
            {
                return Unauthorized();
            }
        }

        [HttpGet("login")]
        public IActionResult Login()
        {
            return Challenge(new AuthenticationProperties { RedirectUri = "/Auth/callback" }, "Steam");
        }

        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        //[HttpGet("api/secure-endpoint")]
        //public IActionResult ApiSecureEndpoint()
        //{
        //    return Ok("Сообщение из защиенного JWT метода");
        //}
        private static string GetSteamFromUrl(string url)
        {
            string pattern = @"\d+";
            Match match = Regex.Match(url, pattern);
            return match.Value;
        }
    }
}
