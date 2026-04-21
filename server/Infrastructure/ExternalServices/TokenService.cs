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
        // Отримання значень з оточення
        var secret = Environment.GetEnvironmentVariable("JWT_SECRET");
        var issuer = Environment.GetEnvironmentVariable("JWT_ISSUER");
        var audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE");
        var expiresIn = Environment.GetEnvironmentVariable("JWT_EXPIRY_MINUTES");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(expiresIn != null ? int.Parse(expiresIn) : 60),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = creds,
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}
