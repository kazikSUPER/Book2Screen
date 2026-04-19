namespace Book2Screen.Domain.Entities;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Подія сюжету (епізод або розділ).
/// </summary>
public class PlotEvent : BaseEntity
{
    public Guid WorkId { get; set; }

    public Work Work { get; set; } = null!;

    /// <summary>
    /// Джерело події (book, adaptation).
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string SourceType { get; set; } = null!;

    [Required]
    [MaxLength(255)]
    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public int SequenceNumber { get; set; }
}
