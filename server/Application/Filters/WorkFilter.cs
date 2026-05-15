// <copyright file="WorkFilter.cs" company="Team 17">
// Copyright (c) Team 17. All rights reserved.
// </copyright>

namespace Book2Screen.Application.Filters;

/// <summary>
/// Параметри фільтрації для творів.
/// </summary>
public class WorkFilter
{
    /// <summary>
    /// Gets or sets пошуковий запит (по назві).
    /// </summary>
    public string? Search { get; set; }

    /// <summary>
    /// Gets or sets жанр.
    /// </summary>
    public string? Genre { get; set; }

    /// <summary>
    /// Gets or sets країну.
    /// </summary>
    public string? Country { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether фільтрувати лише твори з картою розбіжностей.
    /// </summary>
    public bool? OnlyWithMap { get; set; }
}
