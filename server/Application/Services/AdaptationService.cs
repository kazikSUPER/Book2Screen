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
    /// <returns>DTO-об'єкт адаптації.</returns>
    public async Task<AdaptationDto?> GetAdaptationByIdAsync(Guid id)
    {
        var adaptation = await this.context.Adaptations.FindAsync(id);
        if (adaptation == null)
        {
            throw new KeyNotFoundException($"Adaptation with ID {id} not found.");
        }

        return this.mapper.Map<AdaptationDto>(adaptation);
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

    /// <inheritdoc/>
    public async Task<AdaptationDto> CreateAdaptationAsync(AdaptationDto adaptationDto)
    {
        var adaptation = this.mapper.Map<Domain.Entities.Adaptation>(adaptationDto);
        adaptation.Id = Guid.NewGuid();

        // 1. Створюємо Книгу
        var book = new Domain.Entities.Book
        {
            Id = Guid.NewGuid(),
            Title = adaptationDto.Title,
            Description = adaptationDto.Description,
            Genre = adaptationDto.Genre ?? "Драма",
            Authors = !string.IsNullOrEmpty(adaptationDto.Author)
                ? new List<Domain.Entities.Author> { new Domain.Entities.Author { FullName = adaptationDto.Author }, }
                : new List<Domain.Entities.Author>(),
        };

        // 2. Створюємо Твір (Work) та пов'язуємо з Книгою та Адаптацією
        var work = new Domain.Entities.Work
        {
            Id = Guid.NewGuid(),
            Title = adaptationDto.Title,
            BookId = book.Id,
            AdaptationId = adaptation.Id,
            Summary = adaptationDto.Description,
        };

        // 3. Якщо передано розбіжності — створюємо карту
        if (adaptationDto.Differences != null && adaptationDto.Differences.Any())
        {
            var diffMap = new Domain.Entities.DifferenceMap
            {
                Id = Guid.NewGuid(),
                WorkId = work.Id,
                Differences = adaptationDto.Differences.Select(d => new Domain.Entities.Difference
                {
                    Id = Guid.NewGuid(),
                    DifferenceType = d.Title,
                    Description = d.BookText, // Використовуємо BookText як основний опис
                    ImportanceLevel = d.IsSpoiler ? "high" : "medium",
                }).ToList(),
            };
            work.DifferenceMap = diffMap;
            await this.context.DifferenceMaps.AddAsync(diffMap);
        }

        await this.context.Books.AddAsync(book);
        await this.context.Adaptations.AddAsync(adaptation);
        await this.context.Works.AddAsync(work);

        await this.context.SaveChangesAsync();

        return this.mapper.Map<AdaptationDto>(adaptation);
    }

    /// <inheritdoc/>
    public async Task<AdaptationDto?> UpdateAdaptationAsync(Guid id, AdaptationDto adaptationDto)
    {
        var adaptation = await this.context.Adaptations.FindAsync(id);
        if (adaptation == null)
        {
            throw new KeyNotFoundException($"Adaptation with ID {id} not found.");
        }

        this.mapper.Map(adaptationDto, adaptation);
        adaptation.Id = id; // Ensure ID is not changed

        this.context.Adaptations.Update(adaptation);
        await this.context.SaveChangesAsync();

        return this.mapper.Map<AdaptationDto>(adaptation);
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteAdaptationAsync(Guid id)
    {
        var adaptation = await this.context.Adaptations.FindAsync(id);
        if (adaptation == null)
        {
            throw new KeyNotFoundException($"Adaptation with ID {id} not found.");
        }

        this.context.Adaptations.Remove(adaptation);
        await this.context.SaveChangesAsync();

        return true;
    }
}
