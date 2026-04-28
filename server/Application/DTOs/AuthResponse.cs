// <copyright file="AuthResponse.cs" company="Team 17">
// Copyright (c) Team 17. All rights reserved.
// </copyright>

namespace Book2Screen.Application.DTOs;

/// <summary>
/// Відповідь з JWT токеном.
/// </summary>
public class AuthResponse
{
    /// <summary>
    /// Gets or sets JWT токен доступу для авторизації.
    /// </summary>
    public string Token { get; set; } = null!;

    /// <summary>
    /// Gets or sets унікальний ідентифікатор користувача.
    /// </summary>
    public string UserId { get; set; } = null!;

    /// <summary>
    /// Gets or sets електронну пошту користувача.
    /// </summary>
    public string Email { get; set; } = null!;

    /// <summary>
    /// Gets or sets публічне ім'я (нікнейм) користувача.
    /// </summary>
    public string Nickname { get; set; } = null!;
}
