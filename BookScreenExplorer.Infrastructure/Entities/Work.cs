namespace BookScreenExplorer.Infrastructure.Entities;

public class Work : BaseEntity
{
    public Guid BookId { get; set; }
    public Guid AdaptationId { get; set; }
    public string Title { get; set; } = null!;
    public string? Summary { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Book Book { get; set; } = null!;
    public Adaptation Adaptation { get; set; } = null!;
    public Rating? Rating { get; set; }
    public DifferenceMap? DifferenceMap { get; set; }
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<Vote> Votes { get; set; } = new List<Vote>();
    public ICollection<PlotEvent> PlotEvents { get; set; } = new List<PlotEvent>();
}
