// <copyright file="ReviewRequest.cs" company="Team 17">
// Copyright (c) Team 17. All rights reserved.
// </copyright>

namespace Book2Screen.Application.DTOs;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Запит на створення відгуку.
/// </summary>
public class ReviewRequest
{
    /// <summary>
    /// Gets or sets iD твору, до якого додається відгук.
    /// </summary>
    [Required]
    public required Guid WorkId { get; set; }

    /// <summary>
    /// Gets or sets текст відгуку.
    /// </summary>
    [Required]
    [StringLength(2000, MinimumLength = 10)]
    public string Text { get; set; } = null!;

    /// <summary>
    /// Gets or sets a value indicating whether чи містить відгук спойлери.
    /// </summary>
    [Required]
    public required bool IsSpoiler { get; set; }

    /// <summary>
    /// Gets or sets оцінка користувача (від 0 до 10).
    /// </summary>
    [Required]
    [Range(0, 10)]
    public required double Rating { get; set; }

    /// <summary>
    /// Gets or sets тип об'єкта відгуку (book, adaptation, comparison).
    /// </summary>
    [Required]
    [RegularExpression("^(book|adaptation|comparison)$")]
    public string TargetType { get; set; } = "comparison";
}
