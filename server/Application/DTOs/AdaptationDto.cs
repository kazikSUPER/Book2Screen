namespace Book2Screen.Application.DTOs;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Об'єкт передачі даних для адаптації (фільму/серіалу).
/// </summary>
public class AdaptationDto
{
    /// <summary>
    /// Унікальний ідентифікатор.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Назва адаптації.
    /// </summary>
    /// <example>Інтерстеллар</example>
    [Required]
    public string Title { get; set; } = null!;

    /// <summary>
    /// Тип: movie або series.
    /// </summary>
    /// <example>movie</example>
    [Required]
    public string Type { get; set; } = null!;

    /// <summary>
    /// Короткий опис сюжету.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Рік виходу на екрани.
    /// </summary>
    /// <example>2014</example>
    public int? ReleaseYear { get; set; }

    /// <summary>
    /// Тривалість у хвилинах.
    /// </summary>
    public int? DurationMinutes { get; set; }

    /// <summary>
    /// Посилання на зображення постера.
    /// </summary>
    public string? PosterUrl { get; set; }

    /// <summary>
    /// Студія виробництва.
    /// </summary>
    public string? Studio { get; set; }

    /// <summary>
    /// Країна виробництва.
    /// </summary>
    public string? Country { get; set; }
}
