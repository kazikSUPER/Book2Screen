// <copyright file="ReviewResponse.cs" company="Team 17">
// Copyright (c) Team 17. All rights reserved.
// </copyright>

namespace Book2Screen.Application.DTOs;

/// <summary>
/// Відповідь з деталями відгуку.
/// </summary>
public class ReviewResponse
{
    /// <summary>
    /// Gets or sets унікальний ідентифікатор відгуку.
    /// </summary>
    public Guid ReviewId { get; set; }

    /// <summary>
    /// Gets or sets iD твору.
    /// </summary>
    public Guid WorkId { get; set; }

    /// <summary>
    /// Gets or sets iD автора відгуку.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets текст відгуку.
    /// </summary>
    public string Text { get; set; } = null!;

    /// <summary>
    /// Gets or sets a value indicating whether наявність спойлерів.
    /// </summary>
    public bool IsSpoiler { get; set; }

    /// <summary>
    /// Gets or sets оцінка.
    /// </summary>
    public double Rating { get; set; }

    /// <summary>
    /// Gets or sets дата створення.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
