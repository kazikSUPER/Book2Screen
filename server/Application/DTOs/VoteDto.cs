namespace Book2Screen.Application.DTOs;

/// <summary>
/// Запит на голосування.
/// </summary>
public class VoteRequest
{
    /// <summary>
    /// ID твору (Work), за який проводиться голосування.
    /// </summary>
    public Guid WorkId { get; set; }

    /// <summary>
    /// Тип голосу: "book" (книга краща) або "movie" (адаптація краща).
    /// </summary>
    public string VoteType { get; set; } = null!; // "BOOK" or "MOVIE"
}

/// <summary>
/// Відповідь з результатами голосування.
/// </summary>
public class VoteResponse
{
    /// <summary>
    /// ID твору.
    /// </summary>
    public Guid WorkId { get; set; }

    /// <summary>
    /// Загальна кількість голосів.
    /// </summary>
    public int TotalVotes { get; set; }

    /// <summary>
    /// Кількість голосів за книгу.
    /// </summary>
    public int BookVotes { get; set; }

    /// <summary>
    /// Кількість голосів за адаптацію.
    /// </summary>
    public int MovieVotes { get; set; }

    /// <summary>
    /// Відсоток голосів за книгу.
    /// </summary>
    public double BookPercentage { get; set; }

    /// <summary>
    /// Відсоток голосів за адаптацію.
    /// </summary>
    public double MoviePercentage { get; set; }
}
