namespace Book2Screen.API__Web_.Configurations;

public class JwtOptions
{
    public string Secret { get; set; } = null!;
    public string? Issuer { get; set; }
    public string? Audience { get; set; }
    public int ExpiryMinutes { get; set; } = 60;
}
