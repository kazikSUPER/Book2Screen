// <copyright file="UserService.cs" company="Team 17">
// Copyright (c) Team 17. All rights reserved.
// </copyright>

namespace Book2Screen.Application.Services;

using Book2Screen.Application.DTOs;
using Book2Screen.Application.Interfaces;
using Book2Screen.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Сервіс для керування даними користувачів.
/// </summary>
public class UserService : IUserService
{
    private readonly ApplicationDbContext context;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserService"/> class.
    /// </summary>
    /// <param name="context">Контекст БД.</param>
    public UserService(ApplicationDbContext context)
    {
        this.context = context;
    }

    /// <inheritdoc/>
    public async Task<UserProfileDto?> GetProfileAsync(Guid userId)
    {
        var user = await this.context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
        {
            return null;
        }

        return new UserProfileDto
        {
            Username = user.Username,
            Email = user.Email,
            AvatarUrl = user.AvatarUrl,
            JoinedAt = user.CreatedAt,
        };
    }

    /// <inheritdoc/>
    public async Task UpdateProfileAsync(Guid userId, UserProfileDto profileDto)
    {
        var user = await this.context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
        {
            throw new KeyNotFoundException($"User with ID {userId} not found.");
        }

        user.Username = profileDto.Username;
        user.Email = profileDto.Email;

        await this.context.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public async Task UpdateAvatarAsync(Guid userId, string avatarUrl)
    {
        var user = await this.context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
        {
            throw new KeyNotFoundException($"User with ID {userId} not found.");
        }

        user.AvatarUrl = avatarUrl;
        await this.context.SaveChangesAsync();
    }
}
