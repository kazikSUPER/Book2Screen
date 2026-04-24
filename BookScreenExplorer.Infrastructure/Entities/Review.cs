namespace BookScreenExplorer.Infrastructure.Entities;

public class Review : BaseEntity
{
    public Guid? UserId { get; set; }
    public Guid WorkId { get; set; }
    public string TargetType { get; set; } = null!;
    public string Text { get; set; } = null!;
    public int LikesCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public User? User { get; set; }
    public Work Work { get; set; } = null!;
}
