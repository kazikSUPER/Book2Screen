namespace Book2Screen.Domain.Entities;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Автор книги.
/// </summary>
public class Author : BaseEntity
{
    [Required]
    [MaxLength(150)]
    public string FullName { get; set; } = null!;

    public DateTime? BirthDate { get; set; }

    public string? Nationality { get; set; }

    public string? Biography { get; set; }

    public ICollection<Book> Books { get; set; } = new List<Book>();
}
