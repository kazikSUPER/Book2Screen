namespace Book2Screen.Tests.Services;

using System.IdentityModel.Tokens.Jwt;
using API__Web_.Configurations;
using Book2Screen.Domain.Entities;
using Book2Screen.Infrastructure.ExternalServices;
using Xunit;
using Xunit.Abstractions;

[Collection("TokenServiceTests")]
public class TokenServiceTests
{
    private readonly ITestOutputHelper _out;

    private const string TestSecret   = "super-secret-key-for-unit-tests-32chars!!";
    private const string TestIssuer   = "book2screen-test";
    private const string TestAudience = "book2screen-test-client";

    public TokenServiceTests(ITestOutputHelper output) => _out = output;

    private static TokenService CreateService() {
        var options = new JwtOptions
        {
            Issuer = TestIssuer,
            Audience = TestAudience,
            ExpiryMinutes = 120,
            Secret = TestSecret,
        };
        return new TokenService(options);
    }

    private static User MakeUser(string username = "testuser", string role = "user") => new()
    {
        Id           = Guid.NewGuid(),
        Username     = username,
        Email        = $"{username}@test.com",
        PasswordHash = BCrypt.Net.BCrypt.HashPassword("P@ssw0rd"),
        Role         = role,
    };

    // ── TC-UNIT-01 ──────────────────────────────────────────────────────────
    [Fact]
    public void CreateToken_ValidUser_ReturnsNonEmptyJwtString()
    {
        var token = CreateService().CreateToken(MakeUser());
        Assert.NotNull(token);
        Assert.NotEmpty(token);
        Assert.Equal(3, token.Split('.').Length);
    }

    // ── TC-UNIT-02 — Діагностика: виводимо ВСІ claim types у консоль ──────
    [Fact]
    public void CreateToken_ValidUser_ContainsCorrectClaims()
    {
        var user = MakeUser(username: "qauser", role: "admin");
        var jwt  = new JwtSecurityTokenHandler()
                       .ReadJwtToken(CreateService().CreateToken(user));

        // Dump all claims so we can see exact type strings
        foreach (var c in jwt.Claims)
            _out.WriteLine($"  Type='{c.Type}'  Value='{c.Value}'");

        // Actual claim types written by this JWT lib version:
        // ClaimTypes.Name           → "unique_name"
        // ClaimTypes.NameIdentifier → "nameid"
        // ClaimTypes.Role           → "role"
        var name = jwt.Claims.FirstOrDefault(c => c.Type == "unique_name");
        Assert.NotNull(name);
        Assert.Equal("qauser", name!.Value);

        var id = jwt.Claims.FirstOrDefault(c => c.Type == "nameid");
        Assert.NotNull(id);
        Assert.Equal(user.Id.ToString(), id!.Value);

        var role = jwt.Claims.FirstOrDefault(c => c.Type == "role");
        Assert.NotNull(role);
        Assert.Equal("admin", role!.Value);
    }

    // ── TC-UNIT-03 ──────────────────────────────────────────────────────────
    [Fact]
    public void CreateToken_ValidUser_ExpiresInConfiguredMinutes()
    {
        var service    = CreateService();
        var user       = MakeUser();
        var beforeCall = DateTime.UtcNow;
        var jwt        = new JwtSecurityTokenHandler().ReadJwtToken(service.CreateToken(user));
        var afterCall  = DateTime.UtcNow;

        Assert.True(
            jwt.ValidTo >= beforeCall.AddMinutes(119.5) &&
            jwt.ValidTo <= afterCall.AddMinutes(120.5),
            $"Expiry {jwt.ValidTo:O} outside expected window.");
    }

    // ── TC-UNIT-04 ──────────────────────────────────────────────────────────
    [Fact]
    public void CreateToken_SameUserCalledTwice_ReturnsDifferentTokens()
    {
        var service = CreateService();
        var user    = MakeUser();
        var token1  = service.CreateToken(user);
        System.Threading.Thread.Sleep(1100);
        var token2  = service.CreateToken(user);
        Assert.NotEqual(token1, token2);
    }

    // ── TC-UNIT-05 ──────────────────────────────────────────────────────────
    [Fact]
    public void Constructor_MissingSecret_ThrowsInvalidOperationException()
    {
        var user = MakeUser();
        var options = new JwtOptions
        {
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            ExpiryMinutes = 120,
            Secret = null
        };
        Assert.Throws<InvalidOperationException>(() => new TokenService(options));
    }
}
