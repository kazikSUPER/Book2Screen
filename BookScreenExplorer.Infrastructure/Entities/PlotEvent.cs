namespace BookScreenExplorer.Infrastructure.Entities;

public class PlotEvent : BaseEntity
{
    public Guid WorkId { get; set; }
    public string SourceType { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public int SequenceNumber { get; set; }
    public int? EpisodeNumber { get; set; }
    public int? SeasonNumber { get; set; }
    public DateTime CreatedAt { get; set; }

    public Work Work { get; set; } = null!;
    public ICollection<Difference> BookDifferences { get; set; } = new List<Difference>();
    public ICollection<Difference> AdaptationDifferences { get; set; } = new List<Difference>();
}
