using AuthService.Models;
using AuthService.Repositories;
using AuthService.Services;
using AuthService;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Net;

public class MockHttpMessageHandler : HttpMessageHandler
{
    private readonly HttpResponseMessage _mockResponse;

    public MockHttpMessageHandler(HttpResponseMessage response)
    {
        _mockResponse = response;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return Task.FromResult(_mockResponse);
    }
}

[TestFixture]
public class SteamUserServiceTests
{
    private Mock<IUserRepository> _mockUserRepository;
    private Mock<IHttpClientFactory> _mockHttpClientFactory;
    private Mock<ILogger<SteamUserService>> _mockLogger;
    private HttpClient _httpClient;
    private SteamSettings _steamSettings;
    private JwtSettings _jwtSettings;
    private SteamUserService _service;
    private IJwtTokenService _jwtTokenService;

    [SetUp]
    public void Setup()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockHttpClientFactory = new Mock<IHttpClientFactory>();
        _mockLogger = new Mock<ILogger<SteamUserService>>();

        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(@"{""response"": {""players"": [{""steamid"": ""76561198000000000"", ""personaname"": ""TestUser"", ""avatar"": ""http://avatar.url""}]}}")
        };

        _httpClient = new HttpClient(new MockHttpMessageHandler(response))
        {
            BaseAddress = new Uri("https://api.steampowered.com/")
        };
        _mockHttpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(_httpClient);

        _steamSettings = new SteamSettings { ApiKey = "test-key" };
        _jwtSettings = new JwtSettings
        {
            SecretKey = "test_secret_key_1234567890",
            Issuer = "test-issuer",
            Audience = "test-audience",
            ExpireDays = 5
        };
        _jwtTokenService = new JwtTokenService(Options.Create(_jwtSettings));

        _mockUserRepository.Setup(repo => repo.GetUserBySteamIdAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
        _mockUserRepository.Setup(repo => repo.AddUserAsync(It.IsAny<User>())).ReturnsAsync((User?)null);

        _service = new SteamUserService(
            Options.Create(_steamSettings),
            _mockHttpClientFactory.Object,
            _mockUserRepository.Object,
            _mockLogger.Object,
            _jwtTokenService);
    }

    [Test]
    public async Task AuthorizeUserAsync_GeneratesValidToken_WithCorrectClaims()
    {
        var validSteamId = "76561198000000000";

        var token = await _service.AuthorizeUserAsync(validSteamId);

        Assert.IsNotNull(token);

        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadJwtToken(token);

        var steamIdClaim = jsonToken.Claims.FirstOrDefault(claim => claim.Type == JwtRegisteredClaimNames.Sub)?.Value;

        Assert.IsNotNull(jsonToken);
        Assert.AreEqual(validSteamId, steamIdClaim);
    }
}
