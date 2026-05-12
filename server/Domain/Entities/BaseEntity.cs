// <copyright file="BaseEntity.cs" company="Team 17">
// Copyright (c) Team 17. All rights reserved.
// </copyright>

namespace Book2Screen.Domain.Entities;

/// <summary>
/// Базова сутність, що містить спільні поля для всіх таблиць БД.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Gets or sets унікальний ідентифікатор запису (UUID).
    /// </summary>
    /// <example>f47ac10b-58cc-4372-a567-0e02b2c3d479.</example>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets дату та час створення запису (UTC).
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets дату та час останнього оновлення запису (UTC).
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
