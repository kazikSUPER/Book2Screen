// <copyright file="TokenService.cs" company="Team 17">
// Copyright (c) Team 17. All rights reserved.
// </copyright>

namespace Book2Screen.Infrastructure.ExternalServices;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Book2Screen.API__Web_.Configurations;
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
    /// Initializes a new instance of the <see cref="TokenService"/> class.
    /// Ініціалізує новий екземпляр <see cref="TokenService"/> та завантажує налаштування JWT із середовища.
    /// </summary>
    /// <param name="jwtOptions">Налаштування JWT.</param>
    /// <exception cref="InvalidOperationException">Викидається, якщо JWT_SECRET не встановлено.</exception>
    public TokenService(JwtOptions jwtOptions)
    {
        this.secret = jwtOptions.Secret
            ?? throw new InvalidOperationException("JWT_SECRET environment variable is not set.");
        this.issuer = jwtOptions.Issuer;
        this.audience = jwtOptions.Audience;
        this.expiryMinutes = jwtOptions.ExpiryMinutes;
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

        var now = DateTime.UtcNow;

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            NotBefore = now,
            IssuedAt = now,
            Expires = now.AddMinutes(this.expiryMinutes),
            Issuer = this.issuer,
            Audience = this.audience,
            SigningCredentials = creds,
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}
