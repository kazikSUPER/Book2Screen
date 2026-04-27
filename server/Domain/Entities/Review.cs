// <copyright file="Review.cs" company="Team 17">
// Copyright (c) Team 17. All rights reserved.
// </copyright>

namespace Book2Screen.Domain.Entities;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Відгук користувача.
/// </summary>
public class Review : BaseEntity
{
    /// <summary>
    /// Gets or sets iD користувача.
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// Gets or sets об'єкт користувача.
    /// </summary>
    public User? User { get; set; }

    /// <summary>
    /// Gets or sets iD твору.
    /// </summary>
    public Guid WorkId { get; set; }

    /// <summary>
    /// Gets or sets об'єкт твору.
    /// </summary>
    public Work Work { get; set; } = null!;

    /// <summary>
    /// Gets or sets тип об'єкта відгуку (book, adaptation, comparison).
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string TargetType { get; set; } = null!;

    /// <summary>
    /// Gets or sets текст відгуку.
    /// </summary>
    [Required]
    public string Text { get; set; } = null!;

    /// <summary>
    /// Gets or sets a value indicating whether чи містить відгук спойлери.
    /// </summary>
    public bool IsSpoiler { get; set; } = false;

    /// <summary>
    /// Gets or sets оцінку користувача.
    /// </summary>
    public double Rating { get; set; } = 0;

    /// <summary>
    /// Gets or sets кількість лайків.
    /// </summary>
    public int LikesCount { get; set; } = 0;
}
