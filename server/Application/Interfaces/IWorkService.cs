// <copyright file="IWorkService.cs" company="Team 17">
// Copyright (c) Team 17. All rights reserved.
// </copyright>

namespace Book2Screen.Application.Interfaces;

using Book2Screen.Application.DTOs;
using Book2Screen.Application.Filters;

/// <summary>
/// Інтерфейс сервісу для роботи з творами (Works).
/// </summary>
public interface IWorkService
{
    /// <summary>
    /// Отримує список усіх творів з можливістю фільтрації.
    /// </summary>
    /// <param name="filter">Фільтри (пошук, жанр, країна).</param>
    /// <returns>Список DTO творів.</returns>
    Task<IEnumerable<BookScreenItemDto>> GetAllWorksAsync(WorkFilter? filter = null);

    /// <summary>
    /// Отримує деталі твору за ID.
    /// </summary>
    /// <param name="id">ID твору.</param>
    /// <returns>DTO твору або null.</returns>
    Task<BookScreenItemDto?> GetWorkByIdAsync(Guid id);

    /// <summary>
    /// Отримує топ творів за рейтингом адаптації.
    /// </summary>
    /// <param name="count">Кількість творів у топі.</param>
    /// <returns>Відсортований список DTO.</returns>
    Task<IEnumerable<BookScreenItemDto>> GetTopWorksAsync(int count);
}
