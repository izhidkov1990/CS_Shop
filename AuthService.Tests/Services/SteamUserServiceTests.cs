using AuthService.Repositories;
using AuthService.Services;
using AuthService;
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
    private HttpClient _httpClient;
    private SteamSettings _steamSettings;
    private SteamUserService _service;

    [SetUp]
    public void Setup()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockHttpClientFactory = new Mock<IHttpClientFactory>();

        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(@"{""response"": {""players"": [{""steamid"": ""76561199165531336"", ""personaname"": ""TestUser"", ""avatar"": ""http://avatar.url""}]}}")
        };

        _httpClient = new HttpClient(new MockHttpMessageHandler(response));
        _mockHttpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(_httpClient);

        _steamSettings = new SteamSettings { /* инициализация настроек */ };
        _service = new SteamUserService(_steamSettings, _mockHttpClientFactory.Object, _mockUserRepository.Object);
    }

    [Test]
    public async Task AuthorizeUserAsync_GeneratesValidToken_WithCorrectClaims()
    {
        // Arrange
        var validSteamId = "76561199165531336";

        // Act
        var token = await _service.AuthorizeUserAsync(validSteamId);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadJwtToken(token);

        var steamIdClaim = jsonToken.Claims.FirstOrDefault(claim => claim.Type == JwtRegisteredClaimNames.Sub)?.Value;

        Assert.IsNotNull(jsonToken);
        Assert.AreEqual(validSteamId, steamIdClaim);
    }
}

