// <copyright file="FavoriteRequest.cs" company="Team 17">
// Copyright (c) Team 17. All rights reserved.
// </copyright>

namespace Book2Screen.Application.DTOs;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Запит для додавання твору в обране.
/// </summary>
public class FavoriteRequest
{
    /// <summary>
    /// Gets or sets iD твору.
    /// </summary>
    [Required]
    public Guid? WorkId { get; set; }

    /// <summary>
    /// Gets or sets тип (read/watch).
    /// </summary>
    public string Kind { get; set; } = "favorite";
}
