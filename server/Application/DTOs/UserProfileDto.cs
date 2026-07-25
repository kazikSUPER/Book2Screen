// <copyright file="UserProfileDto.cs" company="Team 17">
// Copyright (c) Team 17. All rights reserved.
// </copyright>

namespace Book2Screen.Application.DTOs;

/// <summary>
/// DTO для даних профілю користувача.
/// </summary>
public class UserProfileDto
{
    /// <summary>
    /// Gets or sets ім'я користувача.
    /// </summary>
    public required string Username { get; set; }

    /// <summary>
    /// Gets or sets електронна пошта.
    /// </summary>
    public required string Email { get; set; }

    /// <summary>
    /// Gets or sets URL аватара.
    /// </summary>
    public string? AvatarUrl { get; set; }

    /// <summary>
    /// Gets or sets дату приєднання.
    /// </summary>
    public DateTime? JoinedAt { get; set; }
}
