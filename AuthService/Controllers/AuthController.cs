using AuthService.DTOs;
using AuthService.Models;
using AuthService.Services;
using AutoMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ISteamUserService _authService;
        private readonly IMapper _mapper;
        private readonly ILogger<AuthController> _logger;
        private const string SteamScheme = "Steam";
        private const string SteamClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier";
        private const string CallbackUri = "/Auth/callback";

        public AuthController(ISteamUserService authService, IMapper mapper, ILogger<AuthController> logger)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger; 
        }

        [HttpGet("callback")]
        public async Task<IActionResult> CallbackAsync()
        {
            try
            {
                var claimsPrincipal = HttpContext.User;
                var steamIdClaim = claimsPrincipal.FindFirst(claim => claim.Type == SteamClaimType);
                var steamIdUrl = steamIdClaim?.Value;

                if (!string.IsNullOrEmpty(steamIdUrl))
                {
                    string steamId = _authService.GetSteamFromUrl(steamIdUrl);
                    var jwtToken = await _authService.AuthorizeUserAsync(steamId);
                    if(jwtToken == null)
                    {
                        throw new InvalidOperationException("Can't get token");
                    }
                    var redirectUrl = $"http://localhost:4200/authcallback?token={jwtToken}";
                    return Redirect(redirectUrl);
                }

                return Unauthorized();
            }
            catch (InvalidOperationException invOpEx)
            {
                _logger.LogError(invOpEx, "Ошибка в CallbackAsync");
                return BadRequest("Ошибка аутентификации " + invOpEx.Message) ;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Не известная ошибка в CallbackAsync");
                return BadRequest("Не известная ошибка при авторизации");
            }
        }

        [HttpGet("login")]
        public IActionResult Login()
        {
            return Challenge(new AuthenticationProperties { RedirectUri = CallbackUri }, SteamScheme);
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPut("update_user")]
        public async Task<IActionResult> UpdateUserAsync([FromBody] UserDTO userDTO)
        {
            if (userDTO == null)
            {
                return BadRequest("Данные пользователя не предоставлены.");
            }

            try
            {
                User user = _mapper.Map<User>(userDTO);
                bool result = await _authService.UpdateUserAsync(user);
                return result ? Ok("Пользователь успешно обновлен.") : NotFound("Пользователь не найден.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении пользователя");
                return StatusCode(500, "Ошибка сервера при обновлении пользователя");
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("getuserbyid")]
        public async Task<IActionResult> GetUserByIdAsync()
        {
            var claimsPrincipal = HttpContext.User;
            var steamIdClaim = claimsPrincipal.FindFirst(claim => claim.Type == SteamClaimType);
            var steamIdUrl = steamIdClaim?.Value;

            if (!string.IsNullOrEmpty(steamIdUrl))
            {
                string steamId = _authService.GetSteamFromUrl(steamIdUrl);
                User user = await _authService.GetUserBySteamId(steamId);

                if (user != null)
                {
                    UserDTO userDTO = _mapper.Map<UserDTO>(user);
                    return Ok(userDTO);
                }
            }

            return NotFound("Пользователь не найден.");
        }   
    }
}
