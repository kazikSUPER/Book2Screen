namespace Book2Screen.Application.DTOs;

/// <summary>
/// Об'єкт передачі даних для фронтенду (BookScreenItem).
/// </summary>
public class BookScreenItemDto
{
    /// <summary>
    /// Унікальний ідентифікатор твору.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Назва твору (книги/адаптації).
    /// </summary>
    public string Title { get; set; } = null!;

    /// <summary>
    /// Рік виходу адаптації.
    /// </summary>
    public int Year { get; set; }

    /// <summary>
    /// Основний жанр.
    /// </summary>
    public string Genre { get; set; } = "Драма";

    /// <summary>
    /// Країна виробництва.
    /// </summary>
    public string Country { get; set; } = "Unknown";

    /// <summary>
    /// URL-посилання на постер.
    /// </summary>
    public string Poster { get; set; } = "https://via.placeholder.com/300x450";

    /// <summary>
    /// Середній рейтинг книги.
    /// </summary>
    public double BookRating { get; set; }

    /// <summary>
    /// Середній рейтинг фільму/серіалу.
    /// </summary>
    public double FilmRating { get; set; }

    /// <summary>
    /// Короткий опис сюжету.
    /// </summary>
    public string Description { get; set; } = "";
}
