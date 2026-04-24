namespace BookScreenExplorer.Infrastructure.Entities;

public class Adaptation : BaseEntity
{
    public string Title { get; set; } = null!;
    public string Type { get; set; } = null!;
    public string? Description { get; set; }
    public int? ReleaseYear { get; set; }
    public int? DurationMinutes { get; set; }
    public string? PosterUrl { get; set; }
    public string? Studio { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Work? Work { get; set; }
    public ICollection<AdaptationActor> AdaptationActors { get; set; } = new List<AdaptationActor>();
}
