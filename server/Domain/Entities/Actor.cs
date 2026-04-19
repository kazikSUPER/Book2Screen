namespace Book2Screen.Domain.Entities;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Актор, що знімався в адаптації.
/// </summary>
public class Actor : BaseEntity
{
    [Required]
    [MaxLength(150)]
    public string FullName { get; set; } = null!;

    public DateTime? BirthDate { get; set; }

    public string? Nationality { get; set; }

    public string? Biography { get; set; }

    public ICollection<AdaptationActor> AdaptationActors { get; set; } = new List<AdaptationActor>();
}
