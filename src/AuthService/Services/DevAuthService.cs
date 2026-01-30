using AuthService.DTOs;
using AuthService.Models;
using AuthService.Repositories;

namespace AuthService.Services
{
    public class DevAuthService : IDevAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtTokenService _jwtTokenService;

        public DevAuthService(IUserRepository userRepository, IJwtTokenService jwtTokenService)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _jwtTokenService = jwtTokenService ?? throw new ArgumentNullException(nameof(jwtTokenService));
        }

        public async Task<string?> AuthorizeDevUserAsync(DevLoginRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.SteamId))
            {
                return null;
            }

            var steamId = request.SteamId.Trim();
            var displayName = string.IsNullOrWhiteSpace(request.Name) ? $"DevUser_{steamId}" : request.Name.Trim();

            var existingUser = await _userRepository.GetUserBySteamIdAsync(steamId);
            if (existingUser == null)
            {
                await _userRepository.AddUserAsync(new User
                {
                    Name = displayName,
                    SteamID = steamId,
                    AvatarURL = string.IsNullOrWhiteSpace(request.AvatarUrl) ? null : request.AvatarUrl.Trim(),
                    Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim(),
                    Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim(),
                    DateOfAuth = DateTime.UtcNow
                });
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(request.Name))
                {
                    existingUser.Name = displayName;
                }

                if (!string.IsNullOrWhiteSpace(request.AvatarUrl))
                {
                    existingUser.AvatarURL = request.AvatarUrl.Trim();
                }

                if (!string.IsNullOrWhiteSpace(request.Email))
                {
                    existingUser.Email = request.Email.Trim();
                }

                if (!string.IsNullOrWhiteSpace(request.Phone))
                {
                    existingUser.Phone = request.Phone.Trim();
                }

                existingUser.DateOfAuth = DateTime.UtcNow;
                await _userRepository.UpdateUserAsync(existingUser);
            }

            return _jwtTokenService.GenerateToken(steamId);
        }
    }
}
