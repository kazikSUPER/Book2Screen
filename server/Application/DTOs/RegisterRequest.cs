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
    /// Gets or sets унікальне ім'я користувача (логін).
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Username { get; set; } = null!;

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
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters long")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).*$", ErrorMessage = "Password must contain at least one lowercase letter, one uppercase letter and one digit")]
    public string Password { get; set; } = null!;
}
