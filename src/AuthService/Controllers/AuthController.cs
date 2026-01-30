using AuthService.DTOs;
using AuthService.Models;
using AuthService.Services;
using AutoMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace AuthService.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ISteamUserService _authService;
        private readonly IDevAuthService _devAuthService;
        private readonly IMapper _mapper;
        private readonly ILogger<AuthController> _logger;
        private readonly IHostEnvironment _environment;
        private readonly DevAuthSettings _devAuthSettings;
        private readonly string _frontendCallbackUrl;
        private const string SteamScheme = "Steam";
        private const string CallbackUri = "/Auth/callback";

        public AuthController(
            ISteamUserService authService,
            IDevAuthService devAuthService,
            IMapper mapper,
            ILogger<AuthController> logger,
            IHostEnvironment environment,
            IOptions<DevAuthSettings> devAuthOptions,
            IConfiguration configuration)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _devAuthService = devAuthService ?? throw new ArgumentNullException(nameof(devAuthService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
            _devAuthSettings = devAuthOptions?.Value ?? new DevAuthSettings();
            var configuredCallbackUrl = configuration?["Frontend:AuthCallbackUrl"];
            _frontendCallbackUrl = string.IsNullOrWhiteSpace(configuredCallbackUrl)
                ? "http://localhost:4200/authcallback"
                : configuredCallbackUrl;
        }

        [HttpGet("callback")]
        public async Task<IActionResult> CallbackAsync()
        {
            try
            {
                var authResult = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                if (!authResult.Succeeded || authResult.Principal == null)
                {
                    return Unauthorized();
                }

                var steamIdClaim = authResult.Principal.FindFirst(ClaimTypes.NameIdentifier);
                var steamIdUrl = steamIdClaim?.Value;

                if (!string.IsNullOrEmpty(steamIdUrl))
                {
                    string steamId = _authService.GetSteamFromUrl(steamIdUrl);
                    var jwtToken = await _authService.AuthorizeUserAsync(steamId);
                    if (string.IsNullOrWhiteSpace(jwtToken))
                    {
                        throw new InvalidOperationException("Unable to issue token.");
                    }

                    var separator = _frontendCallbackUrl.Contains('?') ? "&" : "?";
                    var redirectUrl = $"{_frontendCallbackUrl}{separator}token={jwtToken}";
                    return Redirect(redirectUrl);
                }

                return Unauthorized();
            }
            catch (InvalidOperationException invOpEx)
            {
                _logger.LogError(invOpEx, "Error in CallbackAsync");
                return BadRequest("Authentication error: " + invOpEx.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in CallbackAsync");
                return BadRequest("Unexpected error during authorization.");
            }
        }

        [HttpGet("login")]
        public IActionResult Login()
        {
            return Challenge(new AuthenticationProperties { RedirectUri = CallbackUri }, SteamScheme);
        }

        [HttpPost("dev-login")]
        public async Task<IActionResult> DevLoginAsync([FromBody] DevLoginRequest request)
        {
            if (!_environment.IsDevelopment() || !_devAuthSettings.Enabled)
            {
                return NotFound();
            }

            if (!string.IsNullOrWhiteSpace(_devAuthSettings.SharedKey))
            {
                if (!Request.Headers.TryGetValue("X-Dev-Auth-Key", out var providedKey) ||
                    !string.Equals(providedKey, _devAuthSettings.SharedKey, StringComparison.Ordinal))
                {
                    return Unauthorized();
                }
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var token = await _devAuthService.AuthorizeDevUserAsync(request);
            if (string.IsNullOrWhiteSpace(token))
            {
                return BadRequest("Unable to issue token.");
            }

            return Ok(new { token });
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPut("update_user")]
        public async Task<IActionResult> UpdateUserAsync([FromBody] UserUpdateDTO update)
        {
            if (update == null)
            {
                return BadRequest("User data was not provided.");
            }

            try
            {
                var steamIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrWhiteSpace(steamIdClaim))
                {
                    return Unauthorized();
                }

                string steamId = _authService.GetSteamFromUrl(steamIdClaim);
                bool result = await _authService.UpdateUserAsync(steamId, update);
                return result ? Ok("User updated successfully.") : NotFound("User not found.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while updating user");
                return StatusCode(500, "Server error while updating user.");
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("getuserbyid")]
        public async Task<IActionResult> GetUserByIdAsync()
        {
            var claimsPrincipal = HttpContext.User;
            var steamIdClaim = claimsPrincipal.FindFirst(claim => claim.Type == ClaimTypes.NameIdentifier);
            var steamIdUrl = steamIdClaim?.Value;

            if (!string.IsNullOrEmpty(steamIdUrl))
            {
                string steamId = _authService.GetSteamFromUrl(steamIdUrl);
                User? user = await _authService.GetUserBySteamId(steamId);

                if (user != null)
                {
                    UserDTO userDTO = _mapper.Map<UserDTO>(user);
                    return Ok(userDTO);
                }
            }

            return NotFound("User not found.");
        }
    }
}
