// <copyright file="ForgotPasswordRequest.cs" company="Team 17">
// Copyright (c) Team 17. All rights reserved.
// </copyright>

namespace Book2Screen.Application.DTOs;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Запит на відновлення паролю.
/// </summary>
public class ForgotPasswordRequest
{
    /// <summary>
    /// Gets or sets Email користувача.
    /// </summary>
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;
}
