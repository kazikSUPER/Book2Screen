// <copyright file="Book.cs" company="Team 17">
// Copyright (c) Team 17. All rights reserved.
// </copyright>

namespace Book2Screen.Domain.Entities;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Книга, що є основою для порівняння.
/// </summary>
public class Book : BaseEntity
{
    /// <summary>
    /// Gets or sets назву книги.
    /// </summary>
    [Required]
    [MaxLength(255)]
    public string Title { get; set; } = null!;

    /// <summary>
    /// Gets or sets опис книги.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets жанр книги.
    /// </summary>
    public string? Genre { get; set; }

    /// <summary>
    /// Gets or sets рік публікації.
    /// </summary>
    public int? PublicationYear { get; set; }

    /// <summary>
    /// Gets or sets мову книги.
    /// </summary>
    public string? Language { get; set; }

    /// <summary>
    /// Gets or sets посилання на обкладинку.
    /// </summary>
    public string? CoverImageUrl { get; set; }

    /// <summary>
    /// Gets or sets авторів книги.
    /// </summary>
    public ICollection<Author> Authors { get; set; } = new List<Author>();

    /// <summary>
    /// Gets or sets пов'язаний твір.
    /// </summary>
    public Work? Work { get; set; }
}
