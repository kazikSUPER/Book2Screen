// <copyright file="Difference.cs" company="Team 17">
// Copyright (c) Team 17. All rights reserved.
// </copyright>

namespace Book2Screen.Domain.Entities;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Конкретна відмінність в сюжеті.
/// </summary>
public class Difference : BaseEntity
{
    /// <summary>
    /// Gets or sets iD карти відмінностей.
    /// </summary>
    public Guid MapId { get; set; }

    /// <summary>
    /// Gets or sets об'єкт карти відмінностей.
    /// </summary>
    public DifferenceMap Map { get; set; } = null!;

    /// <summary>
    /// Gets or sets iD події в книзі.
    /// </summary>
    public Guid? BookEventId { get; set; }

    /// <summary>
    /// Gets or sets об'єкт події в книзі.
    /// </summary>
    public PlotEvent? BookEvent { get; set; }

    /// <summary>
    /// Gets or sets iD події в адаптації.
    /// </summary>
    public Guid? AdaptationEventId { get; set; }

    /// <summary>
    /// Gets or sets об'єкт події в адаптації.
    /// </summary>
    public PlotEvent? AdaptationEvent { get; set; }

    /// <summary>
    /// Gets or sets заголовок відмінності (сцени).
    /// </summary>
    [Required]
    [MaxLength(255)]
    public string Title { get; set; } = null!;

    /// <summary>
    /// Gets or sets опис у книзі.
    /// </summary>
    [Required]
    public string BookText { get; set; } = null!;

    /// <summary>
    /// Gets or sets опис в екранізації.
    /// </summary>
    [Required]
    public string FilmText { get; set; } = null!;

    /// <summary>
    /// Gets or sets a value indicating whether чи є точка спойлером.
    /// </summary>
    public bool IsSpoiler { get; set; }

    /// <summary>
    /// Gets or sets тип зміни (застаріле, лишаємо для сумісності).
    /// </summary>
    [MaxLength(20)]
    public string? DifferenceType { get; set; }

    /// <summary>
    /// Gets or sets рівень важливості (low, medium, high).
    /// </summary>
    [MaxLength(20)]
    public string ImportanceLevel { get; set; } = "medium";
}
