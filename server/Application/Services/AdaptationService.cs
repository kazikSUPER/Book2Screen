// <copyright file="AdaptationService.cs" company="Team 17">
// Copyright (c) Team 17. All rights reserved.
// </copyright>

namespace Book2Screen.Application.Services;

using AutoFilterer.Extensions;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Book2Screen.Application.DTOs;
using Book2Screen.Application.Filters;
using Book2Screen.Application.Interfaces;
using Book2Screen.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Сервіс для роботи з адаптаціями літературних творів.
/// Забезпечує функціонал отримання, фільтрації та роботи з адаптаціями.
/// </summary>
public class AdaptationService : IAdaptationService
{
    private readonly ApplicationDbContext context;
    private readonly IMapper mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdaptationService"/> class.
    /// Ініціалізує новий <see cref="AdaptationService"/> клас.
    /// </summary>
    /// <param name="context">Контекст бази даних.</param>
    /// <param name="mapper">Мапер для Dto.</param>
    public AdaptationService(ApplicationDbContext context, IMapper mapper)
    {
        this.context = context;
        this.mapper = mapper;
    }

    /// <summary>
    /// Отримує всі адаптації з бази даних.
    /// </summary>
    /// <returns>Колекція DTO-об'єктів адаптацій.</returns>
    public async Task<IEnumerable<AdaptationDto>> GetAllAdaptationsAsync()
    {
        return await this.context.Adaptations
            .ProjectTo<AdaptationDto>(this.mapper.ConfigurationProvider)
            .ToListAsync();
    }

    /// <summary>
    /// Отримує адаптацію за її унікальним ідентифікатором.
    /// </summary>
    /// <param name="id">Унікальний ідентифікатор адаптації (GUID).</param>
    /// <returns>DTO-об'єкт адаптації або null, якщо адаптацію не знайдено.</returns>
    public async Task<AdaptationDto?> GetAdaptationByIdAsync(Guid id)
    {
        var adaptation = await this.context.Adaptations.FindAsync(id);
        return adaptation == null ? null : this.mapper.Map<AdaptationDto>(adaptation);
    }

    /// <summary>
    /// Фільтрує адаптації за заданими критеріями.
    /// </summary>
    /// <param name="filter">Об'єкт фільтру з параметрами для фільтрації адаптацій.</param>
    /// <returns>Відфільтрована колекція DTO-об'єктів адаптацій.</returns>
    public async Task<IEnumerable<AdaptationDto>> GetFilteredAdaptationsAsync(AdaptationFilter filter)
    {
        return await this.context.Adaptations
            .ApplyFilter(filter)
            .ProjectTo<AdaptationDto>(this.mapper.ConfigurationProvider)
            .ToListAsync();
    }
}
