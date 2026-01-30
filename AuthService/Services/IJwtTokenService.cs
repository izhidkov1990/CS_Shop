namespace AuthService.Services
{
    public interface IJwtTokenService
    {
        string GenerateToken(string steamId);
    }
}
