namespace BookScreenExplorer.Infrastructure.Entities;

public class Vote : BaseEntity
{
    public Guid? UserId { get; set; }
    public Guid WorkId { get; set; }
    public string SelectedOption { get; set; } = null!;
    public DateTime CreatedAt { get; set; }

    public User? User { get; set; }
    public Work Work { get; set; } = null!;
}
