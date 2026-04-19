namespace Book2Screen.Domain.Entities;

/// <summary>
/// Рейтинги та статистика голосування.
/// </summary>
public class Rating : BaseEntity
{
    public Guid WorkId { get; set; }

    public Work Work { get; set; } = null!;

    public decimal? BookRating { get; set; }

    public decimal? AdaptationRating { get; set; }

    public int VotesCount { get; set; } = 0;
}
