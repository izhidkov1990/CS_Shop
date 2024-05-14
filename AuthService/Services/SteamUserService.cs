using AuthService.DTOs;
using AuthService.Models;
using AuthService.Repositories;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace AuthService.Services
{
    public class SteamUserService : ISteamUserService
    {
        private readonly SteamSettings _steamSettings;
        private readonly IUserRepository _userRepository;
        private readonly HttpClient _httpClient;

        public SteamUserService(SteamSettings steamSettings, IHttpClientFactory httpClientFactory, IUserRepository userRepository)
        {
            _steamSettings = steamSettings;
            _userRepository = userRepository;
            _httpClient = httpClientFactory.CreateClient();
        }

        public async Task<string> AuthorizeUserAsync(string steamId)
        {
            var url = $"http://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/?key={_steamSettings.ApiKey}&steamids={steamId}";
            try
            {
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
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            var existingUser = _userRepository.GetUserBySteamIdAsync(user.SteamID);
            if(existingUser != null)
            {
                await _userRepository.UpdateUserAsync(user);
                return true;
            }
            else
            {
                return false;
            }
        }

        private string GenerateJwtToken(string steamId)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, steamId),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT_SECRET_KEY")));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddDays(Convert.ToDouble(5));

            var token = new JwtSecurityToken(
                issuer: Environment.GetEnvironmentVariable("JWT_ISSUER"),
                audience: Environment.GetEnvironmentVariable("JWT_AUDIENCE"),
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GetSteamFromUrl(string url)
        {
            string pattern = @"\d+";
            Match match = Regex.Match(url, pattern);
            return match.Value;
        }

        public Task<User> GetUserBySteamId(string steamId)
        {
            return  _userRepository.GetUserBySteamIdAsync(steamId);
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
