namespace BookScreenExplorer.Infrastructure.Entities;

public class AdaptationActor : BaseEntity
{
    public Guid AdaptationId { get; set; }
    public Guid ActorId { get; set; }
    public string? RoleName { get; set; }

    public Adaptation Adaptation { get; set; } = null!;
    public Actor Actor { get; set; } = null!;
}
