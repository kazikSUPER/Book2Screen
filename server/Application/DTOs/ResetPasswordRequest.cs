// <copyright file="ResetPasswordRequest.cs" company="Team 17">
// Copyright (c) Team 17. All rights reserved.
// </copyright>

namespace Book2Screen.Application.DTOs;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Запит на встановлення нового паролю.
/// </summary>
public class ResetPasswordRequest
{
    /// <summary>
    /// Gets or sets Email користувача.
    /// </summary>
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    /// <summary>
    /// Gets or sets код відновлення.
    /// </summary>
    [Required]
    public string Code { get; set; } = null!;

    /// <summary>
    /// Gets or sets новий пароль.
    /// </summary>
    [Required]
    [MinLength(6)]
    public string NewPassword { get; set; } = null!;
}
