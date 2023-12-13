namespace AuthService
{
    public class SteamSettings
    {
        public string? ApiKey { get; set; }
        public string? SteamId { get; set; }
    }

    public class JwtSettings
    {
        public string? SecretKey { get; set; }
        public string? Issuer { get; set; }
        public string? Audience { get; set; }
        public string? ExpireDays { get; set; } 
    }
}
