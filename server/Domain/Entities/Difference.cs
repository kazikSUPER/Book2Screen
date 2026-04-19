namespace Book2Screen.Domain.Entities;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Конкретна відмінність в сюжеті.
/// </summary>
public class Difference : BaseEntity
{
    public Guid MapId { get; set; }

    public DifferenceMap Map { get; set; } = null!;

    public Guid? BookEventId { get; set; }

    public PlotEvent? BookEvent { get; set; }

    public Guid? AdaptationEventId { get; set; }

    public PlotEvent? AdaptationEvent { get; set; }

    /// <summary>
    /// Тип зміни (changed, added, removed).
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string DifferenceType { get; set; } = null!;

    [Required]
    public string Description { get; set; } = null!;

    /// <summary>
    /// Рівень важливості (low, medium, high).
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string ImportanceLevel { get; set; } = null!;
}
