// <copyright file="RegisterRequest.cs" company="Team 17">
// Copyright (c) Team 17. All rights reserved.
// </copyright>

namespace Book2Screen.Application.DTOs;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Реєстрація користувача.
/// </summary>
public class RegisterRequest
{
    /// <summary>
    /// Gets or sets унікальне ім'я користувача (нікнейм).
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Nickname { get; set; } = null!;

    /// <summary>
    /// Gets or sets електронну пошту користувача.
    /// </summary>
    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = null!;

    /// <summary>
    /// Gets or sets пароль користувача.
    /// </summary>
    [Required]
    public string Password { get; set; } = null!;
}
