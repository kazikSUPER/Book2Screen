// <copyright file="PlotEvent.cs" company="Team 17">
// Copyright (c) Team 17. All rights reserved.
// </copyright>

namespace Book2Screen.Domain.Entities;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Подія сюжету (епізод або розділ).
/// </summary>
public class PlotEvent : BaseEntity
{
    /// <summary>
    /// Gets or sets iD твору.
    /// </summary>
    public Guid WorkId { get; set; }

    /// <summary>
    /// Gets or sets об'єкт твору.
    /// </summary>
    public Work Work { get; set; } = null!;

    /// <summary>
    /// Gets or sets джерело події (book, adaptation).
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string SourceType { get; set; } = null!;

    /// <summary>
    /// Gets or sets назву події.
    /// </summary>
    [Required]
    [MaxLength(255)]
    public string Title { get; set; } = null!;

    /// <summary>
    /// Gets or sets опис події.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets порядковий номер події.
    /// </summary>
    public int SequenceNumber { get; set; }
}
