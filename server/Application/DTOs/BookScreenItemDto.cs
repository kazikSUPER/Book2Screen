// <copyright file="BookScreenItemDto.cs" company="Team 17">
// Copyright (c) Team 17. All rights reserved.
// </copyright>

namespace Book2Screen.Application.DTOs;

/// <summary>
/// Об'єкт передачі даних для фронтенду (BookScreenItem).
/// </summary>
public class BookScreenItemDto
{
    /// <summary>
    /// Gets or sets унікальний ідентифікатор твору.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets назву твору (книги/адаптації).
    /// </summary>
    public string Title { get; set; } = null!;

    /// <summary>
    /// Gets or sets рік виходу адаптації.
    /// </summary>
    public int Year { get; set; }

    /// <summary>
    /// Gets or sets основний жанр.
    /// </summary>
    public string Genre { get; set; } = "Драма";

    /// <summary>
    /// Gets or sets країну виробництва.
    /// </summary>
    public string Country { get; set; } = "Unknown";

    /// <summary>
    /// Gets or sets URL-посилання на постер.
    /// </summary>
    public string Poster { get; set; } = "https://via.placeholder.com/300x450";

    /// <summary>
    /// Gets or sets середній рейтинг книги.
    /// </summary>
    public double BookRating { get; set; }

    /// <summary>
    /// Gets or sets середній рейтинг фільму/серіалу.
    /// </summary>
    public double FilmRating { get; set; }

    /// <summary>
    /// Gets or sets короткий опис сюжету.
    /// </summary>
    public string Description { get; set; } = string.Empty;
}
