// <copyright file="IFavoriteService.cs" company="Team 17">
// Copyright (c) Team 17. All rights reserved.
// </copyright>

namespace Book2Screen.Application.Interfaces;

using Book2Screen.Application.DTOs;

/// <summary>
/// Інтерфейс сервісу для керування обраними творами.
/// </summary>
public interface IFavoriteService
{
    /// <summary>
    /// Додає твір в обране для користувача.
    /// </summary>
    /// <param name="userId">ID користувача.</param>
    /// <param name="workId">ID твору.</param>
    /// <param name="kind">Тип (read/watch).</param>
    /// <returns>True, якщо успішно.</returns>
    Task<bool> AddToFavoritesAsync(Guid userId, Guid workId, string kind = "favorite");

    /// <summary>
    /// Видаляє твір з обраного для користувача.
    /// </summary>
    /// <param name="userId">ID користувача.</param>
    /// <param name="workId">ID твору.</param>
    /// <param name="kind">Тип (якщо вказано).</param>
    /// <returns>True, якщо успішно.</returns>
    Task<bool> RemoveFromFavoritesAsync(Guid userId, Guid workId, string? kind = null);

    /// <summary>
    /// Отримує список обраних творів користувача.
    /// </summary>
    /// <param name="userId">ID користувача.</param>
    /// <param name="kind">Фільтр за типом.</param>
    /// <returns>Список DTO обраних творів.</returns>
    Task<IEnumerable<BookScreenItemDto>> GetUserFavoritesAsync(Guid userId, string? kind = null);

    /// <summary>
    /// Перевіряє, чи є твір в обраному у користувача.
    /// </summary>
    /// <param name="userId">ID користувача.</param>
    /// <param name="workId">ID твору.</param>
    /// <param name="kind">Тип.</param>
    /// <returns>True, якщо в обраному.</returns>
    Task<bool> IsFavoriteAsync(Guid userId, Guid workId, string? kind = null);
}
