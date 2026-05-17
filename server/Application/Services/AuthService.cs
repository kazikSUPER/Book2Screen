// <copyright file="AuthService.cs" company="Team 17">
// Copyright (c) Team 17. All rights reserved.
// </copyright>

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
    private readonly IEmailService emailService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthService"/> class.
    /// Ініціалізує новий екземпляр <see cref="AuthService"/>.
    /// </summary>
    /// <param name="context">Контекст бази даних.</param>
    /// <param name="tokenService">Сервіс для роботи з токенами.</param>
    /// <param name="emailService">Сервіс для відправки пошти.</param>
    public AuthService(ApplicationDbContext context, ITokenService tokenService, IEmailService emailService)
    {
        this.context = context;
        this.tokenService = tokenService;
        this.emailService = emailService;
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
            Role = user.Role,
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

        using var transaction = await this.context.Database.BeginTransactionAsync();
        try
        {
            var user = new User
            {
                Username = registerRequest.Nickname,
                Email = registerRequest.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerRequest.Password),
                Role = "user", // Default role
            };

            await this.context.Users.AddAsync(user);
            await this.context.SaveChangesAsync();

            await transaction.CommitAsync();

            return new AuthResponse
            {
                Token = this.tokenService.CreateToken(user),
                UserId = user.Id.ToString(),
                Email = user.Email,
                Nickname = user.Username,
                Role = user.Role,
            };
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        var user = await this.context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user == null)
        {
            return true;
        }

        var code = new Random().Next(100000, 999999).ToString();

        var oldTokens = this.context.PasswordResetTokens.Where(t => t.Email == request.Email);
        this.context.PasswordResetTokens.RemoveRange(oldTokens);

        var resetToken = new PasswordResetToken
        {
            Email = request.Email,
            Code = code,
            ExpiryTime = DateTime.UtcNow.AddMinutes(15),
            IsUsed = false,
        };

        await this.context.PasswordResetTokens.AddAsync(resetToken);
        await this.context.SaveChangesAsync();

        await this.emailService.SendEmailAsync(
            request.Email,
            "Код відновлення паролю — Book2Screen",
            $"<p>Ваш код для відновлення паролю: <b>{code}</b></p><p>Код дійсний протягом 15 хвилин.</p>");

        return true;
    }

    /// <inheritdoc/>
    public async Task<bool> VerifyResetCodeAsync(VerifyCodeRequest request)
    {
        var token = await this.context.PasswordResetTokens
            .FirstOrDefaultAsync(t => t.Email == request.Email && t.Code == request.Code && !t.IsUsed);

        if (token == null || token.ExpiryTime < DateTime.UtcNow)
        {
            return false;
        }

        return true;
    }

    /// <inheritdoc/>
    public async Task<AuthResponse?> ResetPasswordAsync(ResetPasswordRequest request)
    {
        var token = await this.context.PasswordResetTokens
            .FirstOrDefaultAsync(t => t.Email == request.Email && t.Code == request.Code && !t.IsUsed);

        if (token == null || token.ExpiryTime < DateTime.UtcNow)
        {
            return null;
        }

        var user = await this.context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user == null)
        {
            return null;
        }

        using var transaction = await this.context.Database.BeginTransactionAsync();
        try
        {
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            token.IsUsed = true;

            this.context.Users.Update(user);
            this.context.PasswordResetTokens.Update(token);
            await this.context.SaveChangesAsync();

            await transaction.CommitAsync();

            return new AuthResponse
            {
                Token = this.tokenService.CreateToken(user),
                UserId = user.Id.ToString(),
                Email = user.Email,
                Nickname = user.Username,
                Role = user.Role,
            };
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
