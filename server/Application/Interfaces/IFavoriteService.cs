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
    /// <returns>True, якщо успішно.</returns>
    Task<bool> AddToFavoritesAsync(Guid userId, Guid workId);

    /// <summary>
    /// Видаляє твір з обраного для користувача.
    /// </summary>
    /// <param name="userId">ID користувача.</param>
    /// <param name="workId">ID твору.</param>
    /// <returns>True, якщо успішно.</returns>
    Task<bool> RemoveFromFavoritesAsync(Guid userId, Guid workId);

    /// <summary>
    /// Отримує список обраних творів користувача.
    /// </summary>
    /// <param name="userId">ID користувача.</param>
    /// <returns>Список DTO обраних творів.</returns>
    Task<IEnumerable<BookScreenItemDto>> GetUserFavoritesAsync(Guid userId);

    /// <summary>
    /// Перевіряє, чи є твір в обраному у користувача.
    /// </summary>
    /// <param name="userId">ID користувача.</param>
    /// <param name="workId">ID твору.</param>
    /// <returns>True, якщо в обраному.</returns>
    Task<bool> IsFavoriteAsync(Guid userId, Guid workId);
}
