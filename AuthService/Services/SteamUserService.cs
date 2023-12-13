using AuthService.DTOs;
using AuthService.Models;
using AuthService.Repositories;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AuthService.Services
{
    public class SteamUserService : ISteamUserService
    {
        private readonly SteamSettings _steamSettings;
        private readonly JwtSettings _jwtSettings;
        private readonly IUserRepository _userRepository;
        private readonly HttpClient _httpClient;

        public SteamUserService(JwtSettings jwtSettings, SteamSettings steamSettings, IHttpClientFactory httpClientFactory, IUserRepository userRepository)
        {
            _steamSettings = steamSettings;
            _jwtSettings = jwtSettings;
            _userRepository = userRepository;
            _httpClient = httpClientFactory.CreateClient();
        }

        public async Task<string> AuthorizeUserAsync(string steamId)
        {
            var url = $"http://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/?key={_steamSettings.ApiKey}&steamids={steamId}";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var steamResponse = await response.Content.ReadFromJsonAsync<GetSteamUserResponse>();

            if (steamResponse?.Response?.Players == null || !steamResponse.Response.Players.Any())
            {
                return null;
            }

            var player = steamResponse.Response.Players[0];

            await _userRepository.AddUserAsync(new User()
            {
                Name = player.PersonaName,
                SteamID = player.SteamId,
                AvatarURL = player.Avatar,
                DateOfAuth = DateTime.UtcNow

            });

            return GenerateJwtToken(player.SteamId);
        }

        private string GenerateJwtToken(string steamId)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, steamId),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddDays(Convert.ToDouble(_jwtSettings.ExpireDays));

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
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
