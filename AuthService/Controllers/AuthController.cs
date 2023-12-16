using AuthService.DTOs;
using AuthService.Models;
using AuthService.Services;
using AutoMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace AuthService.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ISteamUserService _authService;
        private readonly IMapper _mapper;
        private const string SteamScheme = "Steam";
        private const string SteamClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier";
        private const string CallbackUri = "/Auth/callback";

        public AuthController(ISteamUserService authService, IMapper mapper)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet("callback")]
        public async Task<IActionResult> CallbackAsync()
        {
            var claimsPrincipal = HttpContext.User;
            var steamIdClaim = claimsPrincipal.FindFirst(claim => claim.Type == SteamClaimType);
            var steamIdUrl = steamIdClaim?.Value;

            if (!string.IsNullOrEmpty(steamIdUrl))
            {
                string steamId = _authService.GetSteamFromUrl(steamIdUrl);
                var jwtToken = await _authService.AuthorizeUserAsync(steamId);

                return !string.IsNullOrEmpty(jwtToken) ? Ok(new { token = jwtToken }) : Unauthorized();
            }

            return Unauthorized();
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

            User user = _mapper.Map<User>(userDTO);
            bool result = await _authService.UpdateUserAsync(user);

            return result ? Ok("Пользователь успешно обновлен.") : NotFound("Пользователь не найден.");
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
