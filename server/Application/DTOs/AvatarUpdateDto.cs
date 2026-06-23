// <copyright file="AvatarUpdateDto.cs" company="Team 17">
// Copyright (c) Team 17. All rights reserved.
// </copyright>

namespace Book2Screen.Application.DTOs;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// DTO для оновлення аватара користувача.
/// </summary>
public class AvatarUpdateDto
{
    /// <summary>
    /// Gets or sets URL аватара.
    /// </summary>
    [Required]
    [Url]
    public string AvatarUrl { get; set; } = null!;
}
