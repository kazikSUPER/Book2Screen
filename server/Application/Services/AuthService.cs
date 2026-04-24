namespace Book2Screen.Application.Services;

using Book2Screen.Application.DTOs;
using Book2Screen.Application.Interfaces;
using Book2Screen.Domain.Entities;
using Book2Screen.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Сервіс для керування автентифікацією користувачів.
/// </summary>
public class AuthService : IAuthService
{
    private readonly ApplicationDbContext context;
    private readonly ITokenService tokenService;

    /// <summary>
    /// Ініціалізує новий екземпляр <see cref="AuthService"/>.
    /// </summary>
    public AuthService(ApplicationDbContext context, ITokenService tokenService)
    {
        this.context = context;
        this.tokenService = tokenService;
    }

    /// <inheritdoc/>
    public async Task<AuthResponse?> LoginAsync(LoginDto loginDto)
    {
        var user = await this.context.Users.FirstOrDefaultAsync(u => u.Email == loginDto.Email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
        {
            return null;
        }

        return new AuthResponse
        {
            Token = this.tokenService.CreateToken(user),
            UserId = user.Id.ToString(),
            Email = user.Email,
            Nickname = user.Username,
        };
    }

    /// <inheritdoc/>
    public async Task<AuthResponse?> RegisterAsync(RegisterRequest registerRequest)
    {
        var userExists = await this.context.Users.AnyAsync(u =>
            u.Username == registerRequest.Nickname || u.Email == registerRequest.Email);

        if (userExists)
        {
            return null;
        }

        var user = new User
        {
            Username = registerRequest.Nickname,
            Email = registerRequest.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerRequest.Password),
        };

        await this.context.Users.AddAsync(user);
        await this.context.SaveChangesAsync();

        return new AuthResponse
        {
            Token = this.tokenService.CreateToken(user),
            UserId = user.Id.ToString(),
            Email = user.Email,
            Nickname = user.Username,
        };
    }
}
