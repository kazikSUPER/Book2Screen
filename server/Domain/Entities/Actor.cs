// <copyright file="Actor.cs" company="Team 17">
// Copyright (c) Team 17. All rights reserved.
// </copyright>

namespace Book2Screen.Domain.Entities;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Актор, що знімався в адаптації.
/// </summary>
public class Actor : BaseEntity
{
    /// <summary>
    /// Gets or sets повне ім'я актора.
    /// </summary>
    [Required]
    [MaxLength(150)]
    public string FullName { get; set; } = null!;

    /// <summary>
    /// Gets or sets дату народження.
    /// </summary>
    public DateTime? BirthDate { get; set; }

    /// <summary>
    /// Gets or sets національність.
    /// </summary>
    public string? Nationality { get; set; }

    /// <summary>
    /// Gets or sets коротку біографію.
    /// </summary>
    public string? Biography { get; set; }

    /// <summary>
    /// Gets or sets колекцію зв'язків з адаптаціями.
    /// </summary>
    public ICollection<AdaptationActor> AdaptationActors { get; set; } = new List<AdaptationActor>();
}
