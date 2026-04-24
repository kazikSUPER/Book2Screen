namespace Book2Screen.Application.Interfaces;

using Book2Screen.Application.DTOs;

/// <summary>
/// Інтерфейс сервісу для керування відгуками користувачів.
/// </summary>
public interface IReviewService
{
    /// <summary>
    /// Додає новий відгук до твору від імені користувача.
    /// </summary>
    /// <param name="userId">ID автора відгуку.</param>
    /// <param name="request">Дані відгуку (текст, оцінка тощо).</param>
    /// <returns>Дані створеного відгуку.</returns>
    Task<ReviewResponse> AddReviewAsync(Guid userId, ReviewRequest request);

    /// <summary>
    /// Отримує список усіх відгуків для конкретного твору.
    /// </summary>
    /// <param name="workId">ID твору.</param>
    /// <returns>Колекція відгуків, відсортована за часом створення.</returns>
    Task<IEnumerable<ReviewResponse>> GetReviewsByWorkIdAsync(Guid workId);
}
