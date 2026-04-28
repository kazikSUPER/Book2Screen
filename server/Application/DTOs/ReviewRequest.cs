// <copyright file="ReviewRequest.cs" company="Team 17">
// Copyright (c) Team 17. All rights reserved.
// </copyright>

namespace Book2Screen.Application.DTOs;

/// <summary>
/// Запит на створення відгуку.
/// </summary>
public class ReviewRequest
{
    /// <summary>
    /// Gets or sets iD твору, до якого додається відгук.
    /// </summary>
    public required Guid WorkId { get; set; }

    /// <summary>
    /// Gets or sets текст відгуку.
    /// </summary>
    public string Text { get; set; } = null!;

    /// <summary>
    /// Gets or sets a value indicating whether чи містить відгук спойлери.
    /// </summary>
    public required bool IsSpoiler { get; set; }

    /// <summary>
    /// Gets or sets оцінка користувача (від 0 до 10).
    /// </summary>
    public required double Rating { get; set; }
}
