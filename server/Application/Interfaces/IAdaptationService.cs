// <copyright file="IAdaptationService.cs" company="Team 17">
// Copyright (c) Team 17. All rights reserved.
// </copyright>

namespace Book2Screen.Application.Interfaces;

using Book2Screen.Application.DTOs;
using Book2Screen.Application.Filters;

/// <summary>
/// Інтерфейс сервісу для роботи з адаптаціями.
/// </summary>
public interface IAdaptationService
{
    /// <summary>
    /// Отримує всі адаптації.
    /// </summary>
    /// <returns>Список DTO адаптацій.</returns>
    Task<IEnumerable<AdaptationDto>> GetAllAdaptationsAsync();

    /// <summary>
    /// Отримує адаптацію за її унікальним ідентифікатором.
    /// </summary>
    /// <param name="id">Ідентифікатор (GUID).</param>
    /// <returns>DTO адаптації або null, якщо не знайдено.</returns>
    Task<AdaptationDto?> GetAdaptationByIdAsync(Guid id);

    /// <summary>
    /// Фільтрує адаптації за заданими критеріями.
    /// </summary>
    /// <param name="filter">Об'єкт з параметрами фільтрації.</param>
    /// <returns>Відфільтрований список DTO.</returns>
    Task<IEnumerable<AdaptationDto>> GetFilteredAdaptationsAsync(AdaptationFilter filter);

    /// <summary>
    /// Створює нову адаптацію.
    /// </summary>
    /// <param name="adaptationDto">Дані для створення.</param>
    /// <returns>Створена адаптація.</returns>
    Task<AdaptationDto> CreateAdaptationAsync(AdaptationDto adaptationDto);

    /// <summary>
    /// Оновлює існуючу адаптацію.
    /// </summary>
    /// <param name="id">ID адаптації.</param>
    /// <param name="adaptationDto">Нові дані.</param>
    /// <returns>Оновлена адаптація або null.</returns>
    Task<AdaptationDto?> UpdateAdaptationAsync(Guid id, AdaptationDto adaptationDto);

    /// <summary>
    /// Видаляє адаптацію.
    /// </summary>
    /// <param name="id">ID адаптації.</param>
    /// <returns>True, якщо видалено успішно.</returns>
    Task<bool> DeleteAdaptationAsync(Guid id);
}
