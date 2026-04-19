namespace Book2Screen.Domain.Entities;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Адаптація (фільм або серіал).
/// </summary>
public class Adaptation : BaseEntity
{
    /// <summary>
    /// Назва адаптації.
    /// </summary>
    [Required]
    [MaxLength(255)]
    public string Title { get; set; } = null!;

    /// <summary>
    /// Тип адаптації (movie, series).
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string Type { get; set; } = null!;

    public string? Description { get; set; }

    public int? ReleaseYear { get; set; }

    public int? DurationMinutes { get; set; }

    public string? PosterUrl { get; set; }

    public string? Studio { get; set; }

    // Навігаційні властивості
    public ICollection<AdaptationActor> AdaptationActors { get; set; } = new List<AdaptationActor>();

    public Work? Work { get; set; }
}
