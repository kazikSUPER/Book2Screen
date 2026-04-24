namespace Book2Screen.Application.Interfaces;

using Book2Screen.Application.DTOs;

/// <summary>
/// Інтерфейс сервісу для керування голосуваннями.
/// </summary>
public interface IVoteService
{
    /// <summary>
    /// Реєструє голос користувача за книгу або адаптацію.
    /// </summary>
    /// <param name="userId">ID користувача.</param>
    /// <param name="request">Дані запиту (WorkId та обраний варіант).</param>
    /// <returns>Оновлена статистика голосування для твору.</returns>
    Task<VoteResponse> VoteAsync(Guid userId, VoteRequest request);

    /// <summary>
    /// Отримує актуальну статистику голосів для конкретного твору.
    /// </summary>
    /// <param name="workId">ID твору.</param>
    /// <returns>Об'єкт зі статистикою (кількість голосів та відсотки).</returns>
    Task<VoteResponse> GetVoteStatsAsync(Guid workId);
}
