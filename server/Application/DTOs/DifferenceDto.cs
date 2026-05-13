// <copyright file="DifferenceDto.cs" company="Team 17">
// Copyright (c) Team 17. All rights reserved.
// </copyright>

namespace Book2Screen.Application.DTOs;

/// <summary>
/// Точка інтерактивної карти відмінностей між книгою і екранізацією.
/// Відповідає інтерфейсу DifferencePoint на Frontend.
/// </summary>
public class DifferenceDto
{
    /// <summary>
    /// Gets or sets унікальний ідентифікатор.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets заголовок сцени.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets текст для колонки "Книга".
    /// </summary>
    public string BookText { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets текст для колонки "Екранізація".
    /// </summary>
    public string FilmText { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether чи є точка спойлером.
    /// </summary>
    public bool IsSpoiler { get; set; }
}
