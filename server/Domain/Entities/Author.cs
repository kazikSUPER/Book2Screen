// <copyright file="Author.cs" company="Team 17">
// Copyright (c) Team 17. All rights reserved.
// </copyright>

namespace Book2Screen.Domain.Entities;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Автор книги.
/// </summary>
public class Author : BaseEntity
{
    /// <summary>
    /// Gets or sets повне ім'я автора.
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
    /// Gets or sets колекцію книг автора.
    /// </summary>
    public ICollection<Book> Books { get; set; } = new List<Book>();
}
