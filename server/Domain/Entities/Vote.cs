// <copyright file="Vote.cs" company="Team 17">
// Copyright (c) Team 17. All rights reserved.
// </copyright>

namespace Book2Screen.Domain.Entities;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Голос користувача в опитуванні "Що краще?".
/// </summary>
public class Vote : BaseEntity
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
    /// Gets or sets обраний варіант (book, adaptation).
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string SelectedOption { get; set; } = null!;
}
