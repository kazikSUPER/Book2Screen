namespace Book2Screen.Domain.Entities;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Центральна сутність, що пов'язує Книгу та її Адаптацію.
/// </summary>
public class Work : BaseEntity
{
    public Guid BookId { get; set; }

    public Book Book { get; set; } = null!;

    public Guid AdaptationId { get; set; }

    public Adaptation Adaptation { get; set; } = null!;

    [Required]
    [MaxLength(255)]
    public string Title { get; set; } = null!;

    public string? Summary { get; set; }

    // Зв'язки 1:1 та 1:N
    public Rating? Rating { get; set; }

    public DifferenceMap? DifferenceMap { get; set; }

    public ICollection<Review> Reviews { get; set; } = new List<Review>();

    public ICollection<Vote> Votes { get; set; } = new List<Vote>();

    public ICollection<PlotEvent> PlotEvents { get; set; } = new List<PlotEvent>();
}
