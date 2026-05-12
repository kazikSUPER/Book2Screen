// <copyright file="AdaptationActor.cs" company="Team 17">
// Copyright (c) Team 17. All rights reserved.
// </copyright>

namespace Book2Screen.Domain.Entities;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Проміжна сутність для зв'язку Акторів та Адаптацій (Many-to-Many з додатковим полем RoleName).
/// </summary>
public class AdaptationActor : BaseEntity
{
    /// <summary>
    /// Gets or sets iD адаптації.
    /// </summary>
    public Guid AdaptationId { get; set; }

    /// <summary>
    /// Gets or sets об'єкт адаптації.
    /// </summary>
    public Adaptation Adaptation { get; set; } = null!;

    /// <summary>
    /// Gets or sets iD актора.
    /// </summary>
    public Guid ActorId { get; set; }

    /// <summary>
    /// Gets or sets об'єкт актора.
    /// </summary>
    public Actor Actor { get; set; } = null!;

    /// <summary>
    /// Gets or sets назва ролі актора в цій адаптації.
    /// </summary>
    [MaxLength(150)]
    public string? RoleName { get; set; }
}
