using AuthService.DTOs;
using AuthService.Models;
using AuthService.Repositories;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;

namespace AuthService.Services
{
    public class SteamUserService : ISteamUserService
    {
        private readonly SteamSettings _steamSettings;
        private readonly JwtSettings _jwtSettings;
        private readonly IUserRepository _userRepository;
        private readonly HttpClient _httpClient;
        private readonly ILogger<SteamUserService> _logger;

        public SteamUserService(
            IOptions<SteamSettings> steamOptions,
            IOptions<JwtSettings> jwtOptions,
            IHttpClientFactory httpClientFactory,
            IUserRepository userRepository,
            ILogger<SteamUserService> logger)
        {
            _steamSettings = steamOptions?.Value ?? throw new ArgumentNullException(nameof(steamOptions));
            _jwtSettings = jwtOptions?.Value ?? throw new ArgumentNullException(nameof(jwtOptions));
            _httpClient = httpClientFactory.CreateClient("SteamApi");
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<string?> AuthorizeUserAsync(string steamId)
        {
            if (string.IsNullOrWhiteSpace(steamId))
            {
                return null;
            }

            var url = $"ISteamUser/GetPlayerSummaries/v0002/?key={_steamSettings.ApiKey}&steamids={steamId}";
            try
            {
                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Steam API returned {StatusCode} for steamId {SteamId}.", response.StatusCode, steamId);
                    return null;
                }

                var steamResponse = await response.Content.ReadFromJsonAsync<GetSteamUserResponse>();
                if (steamResponse?.Response?.Players == null || !steamResponse.Response.Players.Any())
                {
                    return null;
                }

                var player = steamResponse.Response.Players[0];
                if (string.IsNullOrWhiteSpace(player.SteamId) || string.IsNullOrWhiteSpace(player.PersonaName))
                {
                    return null;
                }

                var existingUser = await _userRepository.GetUserBySteamIdAsync(player.SteamId);
                if (existingUser == null)
                {
                    await _userRepository.AddUserAsync(new User
                    {
                        Name = player.PersonaName,
                        SteamID = player.SteamId,
                        AvatarURL = player.Avatar,
                        DateOfAuth = DateTime.UtcNow
                    });
                }
                else
                {
                    existingUser.Name = player.PersonaName;
                    existingUser.AvatarURL = player.Avatar;
                    existingUser.DateOfAuth = DateTime.UtcNow;
                    await _userRepository.UpdateUserAsync(existingUser);
                }

                return GenerateJwtToken(player.SteamId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Steam authorization failed for steamId {SteamId}.", steamId);
                return null;
            }
        }

        public async Task<bool> UpdateUserAsync(string steamId, UserUpdateDTO update)
        {
            if (string.IsNullOrWhiteSpace(steamId) || update == null)
            {
                return false;
            }

            var existingUser = await _userRepository.GetUserBySteamIdAsync(steamId);
            if (existingUser == null)
            {
                return false;
            }

            if (update.Email != null)
            {
                existingUser.Email = update.Email;
            }

            if (update.Phone != null)
            {
                existingUser.Phone = update.Phone;
            }

            await _userRepository.UpdateUserAsync(existingUser);
            return true;
        }

        private string GenerateJwtToken(string steamId)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, steamId),
                new Claim(ClaimTypes.NameIdentifier, steamId),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddDays(_jwtSettings.ExpireDays);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GetSteamFromUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return string.Empty;
            }

            string pattern = @"\d+";
            Match match = Regex.Match(url, pattern);
            return match.Value;
        }

        public Task<User?> GetUserBySteamId(string steamId)
        {
            return _userRepository.GetUserBySteamIdAsync(steamId);
        }
    }

    public class SteamUserResponse
    {
        public List<UserDTO>? Players { get; set; }
    }

    public class GetSteamUserResponse
    {
        public SteamUserResponse? Response { get; set; }
    }
}
