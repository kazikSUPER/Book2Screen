namespace Book2Screen.Application.DTOs;

/// <summary>
/// Запит на створення відгуку.
/// </summary>
public class ReviewRequest
{
    /// <summary>
    /// ID твору, до якого додається відгук.
    /// </summary>
    public Guid WorkId { get; set; }

    /// <summary>
    /// Текст відгуку.
    /// </summary>
    public string Text { get; set; } = null!;

    /// <summary>
    /// Чи містить відгук спойлери.
    /// </summary>
    public bool IsSpoiler { get; set; }

    /// <summary>
    /// Оцінка користувача (від 0 до 10).
    /// </summary>
    public double Rating { get; set; }
}

/// <summary>
/// Відповідь з деталями відгуку.
/// </summary>
public class ReviewResponse
{
    /// <summary>
    /// Унікальний ідентифікатор відгуку.
    /// </summary>
    public Guid ReviewId { get; set; }

    /// <summary>
    /// ID твору.
    /// </summary>
    public Guid WorkId { get; set; }

    /// <summary>
    /// ID автора відгуку.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Текст відгуку.
    /// </summary>
    public string Text { get; set; } = null!;

    /// <summary>
    /// Наявність спойлерів.
    /// </summary>
    public bool IsSpoiler { get; set; }

    /// <summary>
    /// Оцінка.
    /// </summary>
    public double Rating { get; set; }

    /// <summary>
    /// Дата створення.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
