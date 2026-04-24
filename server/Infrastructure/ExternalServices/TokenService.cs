namespace Book2Screen.Infrastructure.ExternalServices;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Book2Screen.Application.Interfaces;
using Book2Screen.Domain.Entities;
using Microsoft.IdentityModel.Tokens;

/// <summary>
/// Реалізація сервісу для генерації JWT токенів.
/// </summary>
public class TokenService : ITokenService
{
    private readonly string secret;
    private readonly string? issuer;
    private readonly string? audience;
    private readonly int expiryMinutes;

    /// <summary>
    /// Ініціалізує новий екземпляр <see cref="TokenService"/> та завантажує налаштування JWT із середовища.
    /// </summary>
    /// <exception cref="InvalidOperationException">Викидається, якщо JWT_SECRET не встановлено.</exception>
    public TokenService()
    {
        this.secret = Environment.GetEnvironmentVariable("JWT_SECRET") 
            ?? throw new InvalidOperationException("JWT_SECRET environment variable is not set.");
        this.issuer = Environment.GetEnvironmentVariable("JWT_ISSUER");
        this.audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE");
        var expiresIn = Environment.GetEnvironmentVariable("JWT_EXPIRY_MINUTES");
        this.expiryMinutes = expiresIn != null ? int.Parse(expiresIn) : 60;
    }

    /// <summary>
    /// Створює підписаний JWT токен на основі даних користувача.
    /// </summary>
    /// <remarks>
    /// Токен включає наступні Claims:
    /// - <c>Name</c>: Юзернейм користувача.
    /// - <c>NameIdentifier</c>: Унікальний ідентифікатор користувача (ID).
    ///
    /// Термін дії токена: 2 години.
    /// </remarks>
    /// <param name="user">Сутність користувача, для якої генерується токен.</param>
    /// <returns>Рядок JWT токена у форматі Base64.</returns>
    /// <exception cref="ArgumentNullException">Викидається, якщо секретний ключ або дані користувача відсутні.</exception>
    public string CreateToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this.secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Role, user.Role ?? "user"),
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(this.expiryMinutes),
            Issuer = this.issuer,
            Audience = this.audience,
            SigningCredentials = creds,
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}
