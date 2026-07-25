// <copyright file="Report.cs" company="Team 17">
// Copyright (c) Team 17. All rights reserved.
// </copyright>

namespace Book2Screen.Domain.Entities;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Скарга на відгук користувача.
/// </summary>
public class Report : BaseEntity
{
    /// <summary>
    /// Gets or sets iD відгуку, на який скаржаться.
    /// </summary>
    public Guid? ReviewId { get; set; }

    /// <summary>
    /// Gets or sets об'єкт відгуку.
    /// </summary>
    public Review? Review { get; set; }

    /// <summary>
    /// Gets or sets iD користувача, який подав скаргу.
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// Gets or sets об'єкт користувача.
    /// </summary>
    public User? User { get; set; }

    /// <summary>
    /// Gets or sets причину скарги.
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string Reason { get; set; } = null!;

    /// <summary>
    /// Gets or sets статус скарги (Pending, Resolved, Dismissed).
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "Pending";
}
