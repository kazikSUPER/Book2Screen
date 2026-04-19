namespace Book2Screen.Domain.Entities;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Книга, що є основою для порівняння.
/// </summary>
public class Book : BaseEntity
{
    /// <summary>
    /// Назва книги.
    /// </summary>
    [Required]
    [MaxLength(255)]
    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string? Genre { get; set; }

    public int? PublicationYear { get; set; }

    public string? Language { get; set; }

    public string? CoverImageUrl { get; set; }

    // Навігаційні властивості
    public ICollection<Author> Authors { get; set; } = new List<Author>();

    public Work? Work { get; set; }
}
