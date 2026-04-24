namespace BookScreenExplorer.Infrastructure.Entities;

public class Actor : BaseEntity
{
    public string FullName { get; set; } = null!;
    public DateOnly? BirthDate { get; set; }
    public string? Nationality { get; set; }
    public string? Biography { get; set; }

    public ICollection<AdaptationActor> AdaptationActors { get; set; } = new List<AdaptationActor>();
}
