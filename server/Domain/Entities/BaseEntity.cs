namespace Book2Screen.Domain.Entities;

/// <summary>
/// Базова сутність, що містить спільні поля для всіх таблиць БД.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Унікальний ідентифікатор запису (UUID).
    /// </summary>
    /// <example>f47ac10b-58cc-4372-a567-0e02b2c3d479</example>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Дата та час створення запису (UTC).
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Дата та час останнього оновлення запису (UTC).
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
