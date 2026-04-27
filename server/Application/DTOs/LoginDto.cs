// <copyright file="LoginDto.cs" company="Team 17">
// Copyright (c) Team 17. All rights reserved.
// </copyright>

namespace Book2Screen.Application.DTOs;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Авторизація користувача.
/// </summary>
public class LoginDto
{
    /// <summary>
    /// Gets or sets електронну пошту користувача.
    /// </summary>
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    /// <summary>
    /// Gets or sets пароль користувача.
    /// </summary>
    [Required]
    public string Password { get; set; } = null!;
}
