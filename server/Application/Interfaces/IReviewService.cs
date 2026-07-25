// <copyright file="IReviewService.cs" company="Team 17">
// Copyright (c) Team 17. All rights reserved.
// </copyright>

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

    /// <summary>
    /// Оновлює існуючий відгук користувача.
    /// </summary>
    /// <param name="userId">ID користувача (для перевірки власності).</param>
    /// <param name="reviewId">ID відгуку.</param>
    /// <param name="request">Нові дані відгуку.</param>
    /// <returns>True, якщо оновлення успішне.</returns>
    Task<bool> UpdateReviewAsync(Guid userId, Guid reviewId, ReviewRequest request);

    /// <summary>
    /// Видаляє відгук користувача.
    /// </summary>
    /// <param name="userId">ID користувача (для перевірки власності).</param>
    /// <param name="reviewId">ID відгуку.</param>
    /// <returns>True, якщо видалення успішне.</returns>
    Task<bool> DeleteReviewAsync(Guid userId, Guid reviewId);

    /// <summary>
    /// Отримує список відгуків конкретного користувача.
    /// </summary>
    /// <param name="userId">ID користувача.</param>
    /// <returns>Колекція відгуків користувача.</returns>
    Task<IEnumerable<ReviewResponse>> GetUserReviewsAsync(Guid userId);

    /// <summary>
    /// Надсилає скаргу на відгук.
    /// </summary>
    /// <param name="userId">ID користувача, який скаржиться.</param>
    /// <param name="reviewId">ID відгуку.</param>
    /// <param name="reason">Причина скарги.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task ReportReviewAsync(Guid userId, Guid reviewId, string reason);

    /// <summary>
    /// Отримує список усіх скарг (для адміна).
    /// </summary>
    /// <returns>Колекція скарг.</returns>
    Task<IEnumerable<ReportResponse>> GetAllReportsAsync();

    /// <summary>
    /// Модерація відгуку за скаргою.
    /// </summary>
    /// <param name="reportId">ID скарги.</param>
    /// <param name="action">Дія (approve/reject/spoiler).</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task ModerateReviewAsync(Guid reportId, string action);
}
