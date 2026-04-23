namespace Book2Screen.Application.Services;

using Book2Screen.Application.DTOs;
using Book2Screen.Application.Interfaces;
using Book2Screen.Domain.Entities;
using Book2Screen.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext context;
    private readonly ITokenService tokenService;

    public AuthService(ApplicationDbContext context, ITokenService tokenService)
    {
        this.context = context;
        this.tokenService = tokenService;
    }

    public async Task<string?> LoginAsync(LoginDto loginDto)
    {
        var user = await this.context.Users.FirstOrDefaultAsync(u => u.Username == loginDto.Username);

        if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
        {
            return null;
        }

        return this.tokenService.CreateToken(user);
    }

    public async Task<string?> RegisterAsync(RegisterRequest registerRequest)
    {
        var userExists = await this.context.Users.AnyAsync(u =>
            u.Username == registerRequest.Username || u.Email == registerRequest.Email);

        if (userExists)
        {
            return null;
        }

        var user = new User
        {
            Username = registerRequest.Username,
            Email = registerRequest.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerRequest.Password),
        };

        await this.context.Users.AddAsync(user);
        await this.context.SaveChangesAsync();

        return this.tokenService.CreateToken(user);
    }
}
