namespace BookScreenExplorer.Infrastructure.Entities;

public class Difference : BaseEntity
{
    public Guid MapId { get; set; }
    public Guid? BookEventId { get; set; }
    public Guid? AdaptationEventId { get; set; }
    public string DifferenceType { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string ImportanceLevel { get; set; } = null!;
    public DateTime CreatedAt { get; set; }

    public DifferenceMap Map { get; set; } = null!;
    public PlotEvent? BookEvent { get; set; }
    public PlotEvent? AdaptationEvent { get; set; }
}
