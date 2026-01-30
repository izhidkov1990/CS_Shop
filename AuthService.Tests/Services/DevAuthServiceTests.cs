using AuthService.DTOs;
using AuthService.Models;
using AuthService.Repositories;
using AuthService.Services;
using Microsoft.Extensions.Options;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;

[TestFixture]
public class DevAuthServiceTests
{
    private Mock<IUserRepository> _mockUserRepository;
    private JwtSettings _jwtSettings;
    private IJwtTokenService _jwtTokenService;
    private DevAuthService _service;

    [SetUp]
    public void Setup()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _jwtSettings = new JwtSettings
        {
            SecretKey = "test_secret_key_1234567890",
            Issuer = "test-issuer",
            Audience = "test-audience",
            ExpireDays = 5
        };
        _jwtTokenService = new JwtTokenService(Options.Create(_jwtSettings));
        _service = new DevAuthService(_mockUserRepository.Object, _jwtTokenService);
    }

    [Test]
    public async Task AuthorizeDevUserAsync_ReturnsNull_WhenSteamIdMissing()
    {
        var request = new DevLoginRequest { SteamId = " " };

        var token = await _service.AuthorizeDevUserAsync(request);

        Assert.IsNull(token);
    }

    [Test]
    public async Task AuthorizeDevUserAsync_CreatesUser_WhenNotExists()
    {
        var request = new DevLoginRequest
        {
            SteamId = "76561198000000001",
            Name = "Dev User",
            Email = "dev@example.com",
            Phone = "+1234567890",
            AvatarUrl = "http://avatar.url"
        };

        _mockUserRepository.Setup(repo => repo.GetUserBySteamIdAsync(request.SteamId))
            .ReturnsAsync((User?)null);

        User? addedUser = null;
        _mockUserRepository.Setup(repo => repo.AddUserAsync(It.IsAny<User>()))
            .Callback<User>(u => addedUser = u)
            .ReturnsAsync((User?)null);

        var token = await _service.AuthorizeDevUserAsync(request);

        Assert.IsNotNull(token);
        Assert.IsNotNull(addedUser);
        Assert.AreEqual(request.SteamId, addedUser!.SteamID);
        Assert.AreEqual(request.Name, addedUser.Name);
        Assert.AreEqual(request.Email, addedUser.Email);
        Assert.AreEqual(request.Phone, addedUser.Phone);
        Assert.AreEqual(request.AvatarUrl, addedUser.AvatarURL);

        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadJwtToken(token);
        var steamIdClaim = jsonToken.Claims.FirstOrDefault(claim => claim.Type == JwtRegisteredClaimNames.Sub)?.Value;
        Assert.AreEqual(request.SteamId, steamIdClaim);
    }

    [Test]
    public async Task AuthorizeDevUserAsync_UpdatesUser_WhenExists()
    {
        var existingUser = new User
        {
            SteamID = "76561198000000002",
            Name = "Old Name",
            Email = "old@example.com",
            Phone = "+10000000000",
            AvatarURL = "http://old.avatar"
        };
        var request = new DevLoginRequest
        {
            SteamId = existingUser.SteamID,
            Name = "New Name",
            Email = "new@example.com",
            Phone = "+19999999999",
            AvatarUrl = "http://new.avatar"
        };

        _mockUserRepository.Setup(repo => repo.GetUserBySteamIdAsync(existingUser.SteamID))
            .ReturnsAsync(existingUser);

        User? updatedUser = null;
        _mockUserRepository.Setup(repo => repo.UpdateUserAsync(It.IsAny<User>()))
            .Callback<User>(u => updatedUser = u)
            .ReturnsAsync(existingUser);

        var token = await _service.AuthorizeDevUserAsync(request);

        Assert.IsNotNull(token);
        Assert.IsNotNull(updatedUser);
        Assert.AreEqual(request.Name, updatedUser!.Name);
        Assert.AreEqual(request.Email, updatedUser.Email);
        Assert.AreEqual(request.Phone, updatedUser.Phone);
        Assert.AreEqual(request.AvatarUrl, updatedUser.AvatarURL);
    }
}
