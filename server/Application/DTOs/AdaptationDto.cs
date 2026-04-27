// <copyright file="AdaptationDto.cs" company="Team 17">
// Copyright (c) Team 17. All rights reserved.
// </copyright>

namespace Book2Screen.Application.DTOs;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Об'єкт передачі даних для адаптації (фільму/серіалу).
/// </summary>
public class AdaptationDto
{
    /// <summary>
    /// Gets or sets унікальний ідентифікатор.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets назву адаптації.
    /// </summary>
    /// <example>Інтерстеллар.</example>
    [Required]
    public string Title { get; set; } = null!;

    /// <summary>
    /// Gets or sets тип: movie або series.
    /// </summary>
    /// <example>movie.</example>
    [Required]
    public string Type { get; set; } = null!;

    /// <summary>
    /// Gets or sets короткий опис сюжету.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets рік виходу на екрани.
    /// </summary>
    /// <example>2014.</example>
    public int? ReleaseYear { get; set; }

    /// <summary>
    /// Gets or sets тривалість у хвилинах.
    /// </summary>
    public int? DurationMinutes { get; set; }

    /// <summary>
    /// Gets or sets посилання на зображення постера.
    /// </summary>
    public string? PosterUrl { get; set; }

    /// <summary>
    /// Gets or sets студію виробництва.
    /// </summary>
    public string? Studio { get; set; }

    /// <summary>
    /// Gets or sets країну виробництва.
    /// </summary>
    public string? Country { get; set; }
}
