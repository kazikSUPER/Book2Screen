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
    /// Gets or sets тип зміни (changed, added, removed).
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string DifferenceType { get; set; } = null!;

    /// <summary>
    /// Gets or sets опис відмінності.
    /// </summary>
    [Required]
    public string Description { get; set; } = null!;

    /// <summary>
    /// Gets or sets рівень важливості (low, medium, high).
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string ImportanceLevel { get; set; } = null!;
}
