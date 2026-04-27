// <copyright file="Adaptation.cs" company="Team 17">
// Copyright (c) Team 17. All rights reserved.
// </copyright>

namespace Book2Screen.Domain.Entities;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Адаптація (фільм або серіал).
/// </summary>
public class Adaptation : BaseEntity
{
    /// <summary>
    /// Gets or sets назву адаптації.
    /// </summary>
    [Required]
    [MaxLength(255)]
    public string Title { get; set; } = null!;

    /// <summary>
    /// Gets or sets тип адаптації (movie, series).
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string Type { get; set; } = null!;

    /// <summary>
    /// Gets or sets опис адаптації.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets рік виходу.
    /// </summary>
    public int? ReleaseYear { get; set; }

    /// <summary>
    /// Gets or sets тривалість у хвилинах.
    /// </summary>
    public int? DurationMinutes { get; set; }

    /// <summary>
    /// Gets or sets посилання на постер.
    /// </summary>
    public string? PosterUrl { get; set; }

    /// <summary>
    /// Gets or sets студію.
    /// </summary>
    public string? Studio { get; set; }

    /// <summary>
    /// Gets or sets країну.
    /// </summary>
    public string? Country { get; set; }

    /// <summary>
    /// Gets or sets колекцію акторів адаптації.
    /// </summary>
    public ICollection<AdaptationActor> AdaptationActors { get; set; } = new List<AdaptationActor>();

    /// <summary>
    /// Gets or sets пов'язаний твір.
    /// </summary>
    public Work? Work { get; set; }
}
