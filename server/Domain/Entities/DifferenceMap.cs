// <copyright file="DifferenceMap.cs" company="Team 17">
// Copyright (c) Team 17. All rights reserved.
// </copyright>

namespace Book2Screen.Domain.Entities;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Карта відмінностей між книгою та адаптацією.
/// </summary>
public class DifferenceMap : BaseEntity
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
    /// Gets or sets назву карти.
    /// </summary>
    [Required]
    [MaxLength(255)]
    public string Title { get; set; } = null!;

    /// <summary>
    /// Gets or sets версію карти.
    /// </summary>
    public int Version { get; set; } = 1;

    /// <summary>
    /// Gets or sets колекцію відмінностей.
    /// </summary>
    public ICollection<Difference> Differences { get; set; } = new List<Difference>();
}
