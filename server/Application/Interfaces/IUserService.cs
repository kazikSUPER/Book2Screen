// <copyright file="IUserService.cs" company="Team 17">
// Copyright (c) Team 17. All rights reserved.
// </copyright>

namespace Book2Screen.Application.Interfaces;

using Book2Screen.Application.DTOs;

/// <summary>
/// Інтерфейс сервісу для керування даними користувачів.
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Отримує дані профілю користувача.
    /// </summary>
    /// <param name="userId">ID користувача.</param>
    /// <returns>Дані профілю.</returns>
    Task<UserProfileDto?> GetProfileAsync(Guid userId);

    /// <summary>
    /// Оновлює дані профілю користувача.
    /// </summary>
    /// <param name="userId">ID користувача.</param>
    /// <param name="profileDto">Нові дані профілю.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task UpdateProfileAsync(Guid userId, UserProfileDto profileDto);

    /// <summary>
    /// Оновлює аватар користувача.
    /// </summary>
    /// <param name="userId">ID користувача.</param>
    /// <param name="avatarUrl">URL нового аватара.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task UpdateAvatarAsync(Guid userId, string avatarUrl);
}
