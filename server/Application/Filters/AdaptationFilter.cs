namespace Book2Screen.Application.Filters;

using AutoFilterer.Attributes;
using AutoFilterer.Enums;
using AutoFilterer.Types;

/// <summary>
/// Об'єкт фільтрації для пошуку адаптацій за різними критеріями.
/// </summary>
public class AdaptationFilter : FilterBase
{
    /// <summary>
    /// Фільтр за назвою. Шукає збіги за частиною слова (LIKE %value%).
    /// </summary>
    /// <example>Lord of the Rings</example>
    [StringFilterOptions(StringFilterOption.Contains)]
    public string? Title { get; set; }

    /// <summary>
    /// Тип адаптації (наприклад: movie, series, anime).
    /// </summary>
    /// <example>movie</example>
    public string? Type { get; set; }

    /// <summary>
    /// Країна виробництва.
    /// </summary>
    /// <example>USA</example>
    public string? Country { get; set; }

    /// <summary>
    /// Студія, що займалася виробництвом.
    /// </summary>
    /// <example>Warner Bros.</example>
    public string? Studio { get; set; }

    /// <summary>
    /// Діапазон років виходу.
    /// Використовуйте 'ReleaseYear.Min' та 'ReleaseYear.Max'.
    /// </summary>
    /// <remarks>
    /// Приклад в URL: ?ReleaseYear.Min=2010&amp;ReleaseYear.Max=2020
    /// </remarks>
    public Range<int>? ReleaseYear { get; set; }

    /// <summary>
    /// Поле для сортування (наприклад: Title, ReleaseYear).
    /// </summary>
    /// <example>Title</example>
    public string? Sort { get; set; }
}
