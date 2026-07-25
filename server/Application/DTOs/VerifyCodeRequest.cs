// <copyright file="VerifyCodeRequest.cs" company="Team 17">
// Copyright (c) Team 17. All rights reserved.
// </copyright>

namespace Book2Screen.Application.DTOs;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Запит на перевірку коду відновлення.
/// </summary>
public class VerifyCodeRequest
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
}
