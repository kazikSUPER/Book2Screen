namespace BookScreenExplorer.Infrastructure.Entities;

public class Rating : BaseEntity
{
    public Guid WorkId { get; set; }
    public decimal? BookRating { get; set; }
    public decimal? AdaptationRating { get; set; }
    public int VotesCount { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Work Work { get; set; } = null!;
}
