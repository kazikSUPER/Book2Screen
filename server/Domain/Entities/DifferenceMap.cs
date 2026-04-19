namespace Book2Screen.Domain.Entities;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Карта відмінностей між книгою та адаптацією.
/// </summary>
public class DifferenceMap : BaseEntity
{
    public Guid WorkId { get; set; }

    public Work Work { get; set; } = null!;

    [Required]
    [MaxLength(255)]
    public string Title { get; set; } = null!;

    public int Version { get; set; } = 1;

    public ICollection<Difference> Differences { get; set; } = new List<Difference>();
}
