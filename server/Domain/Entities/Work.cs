// <copyright file="Work.cs" company="Team 17">
// Copyright (c) Team 17. All rights reserved.
// </copyright>

namespace Book2Screen.Domain.Entities;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Центральна сутність, що пов'язує Книгу та її Адаптацію.
/// </summary>
public class Work : BaseEntity
{
    /// <summary>
    /// Gets or sets iD книги.
    /// </summary>
    public Guid BookId { get; set; }

    /// <summary>
    /// Gets or sets об'єкт книги.
    /// </summary>
    public Book Book { get; set; } = null!;

    /// <summary>
    /// Gets or sets iD адаптації.
    /// </summary>
    public Guid AdaptationId { get; set; }

    /// <summary>
    /// Gets or sets об'єкт адаптації.
    /// </summary>
    public Adaptation Adaptation { get; set; } = null!;

    /// <summary>
    /// Gets or sets загальну назву твору.
    /// </summary>
    [Required]
    [MaxLength(255)]
    public string Title { get; set; } = null!;

    /// <summary>
    /// Gets or sets короткий опис.
    /// </summary>
    public string? Summary { get; set; }

    /// <summary>
    /// Gets or sets рейтинг твору.
    /// </summary>
    public Rating? Rating { get; set; }

    /// <summary>
    /// Gets or sets карту відмінностей.
    /// </summary>
    public DifferenceMap? DifferenceMap { get; set; }

    /// <summary>
    /// Gets or sets відгуки до твору.
    /// </summary>
    public ICollection<Review> Reviews { get; set; } = new List<Review>();

    /// <summary>
    /// Gets or sets голоси користувачів.
    /// </summary>
    public ICollection<Vote> Votes { get; set; } = new List<Vote>();

    /// <summary>
    /// Gets or sets події сюжету.
    /// </summary>
    public ICollection<PlotEvent> PlotEvents { get; set; } = new List<PlotEvent>();
}
