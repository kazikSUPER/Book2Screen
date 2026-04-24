namespace BookScreenExplorer.Infrastructure.Entities;

public class DifferenceMap : BaseEntity
{
    public Guid WorkId { get; set; }
    public string Title { get; set; } = null!;
    public int Version { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Work Work { get; set; } = null!;
    public ICollection<Difference> Differences { get; set; } = new List<Difference>();
}
