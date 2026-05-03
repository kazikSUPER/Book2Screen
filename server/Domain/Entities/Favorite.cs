// <copyright file="Favorite.cs" company="Team 17">
// Copyright (c) Team 17. All rights reserved.
// </copyright>

namespace Book2Screen.Domain.Entities;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Сутність для представлення "Обраного" користувача.
/// </summary>
public class Favorite : BaseEntity
{
    /// <summary>
    /// Gets or sets iD користувача.
    /// </summary>
    [Required]
    public Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets об'єкт користувача.
    /// </summary>
    public User User { get; set; } = null!;

    /// <summary>
    /// Gets or sets iD твору.
    /// </summary>
    [Required]
    public Guid WorkId { get; set; }

    /// <summary>
    /// Gets or sets об'єкт твору.
    /// </summary>
    public Work Work { get; set; } = null!;
}
