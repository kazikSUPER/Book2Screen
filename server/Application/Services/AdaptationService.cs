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
        var adaptation = await this.context.Adaptations
            .Include(a => a.Work)
                .ThenInclude(w => w!.Book)
                    .ThenInclude(b => b.Authors)
            .Include(a => a.Work)
                .ThenInclude(w => w!.DifferenceMap)
                    .ThenInclude(dm => dm!.Differences)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (adaptation == null)
        {
            throw new KeyNotFoundException($"Adaptation with ID {id} not found.");
        }

        var dto = this.mapper.Map<AdaptationDto>(adaptation);

        // Ручне мапування полів, які не можна транслювати в SQL через ProjectTo, але можна через звичайний Map
        if (adaptation.Work != null && adaptation.Work.Book != null)
        {
            dto.Genre = adaptation.Work.Book.Genre;
            dto.Author = string.Join(", ", adaptation.Work.Book.Authors.Select(a => a.FullName));
            if (adaptation.Work.DifferenceMap != null)
            {
                dto.Differences = adaptation.Work.DifferenceMap.Differences.Select(d => new DifferenceDto
                {
                    Id = d.Id,
                    Title = d.Title,
                    BookText = d.BookText,
                    FilmText = d.FilmText,
                    IsSpoiler = d.IsSpoiler,
                }).ToList();
            }
        }

        return dto;
    }

    /// <summary>
    /// Фільтрує адаптації за заданими критеріями.
    /// </summary>
    /// <param name="filter">Об'єкт фільтру з параметрами для фільтрації адаптацій.</param>
    /// <returns>Відфільтрована колекція DTO-об'єктів адаптацій.</returns>
    public async Task<IEnumerable<AdaptationDto>> GetFilteredAdaptationsAsync(AdaptationFilter filter)
    {
        var adaptations = await this.context.Adaptations
            .ApplyFilter(filter)
            .Include(a => a.Work)
                .ThenInclude(w => w!.Book)
                    .ThenInclude(b => b.Authors)
            .ToListAsync();

        return adaptations.Select(a =>
        {
            var dto = this.mapper.Map<AdaptationDto>(a);
            if (a.Work != null && a.Work.Book != null)
            {
                dto.Genre = a.Work.Book.Genre;
                dto.Author = string.Join(", ", a.Work.Book.Authors.Select(a => a.FullName));
            }

            return dto;
        });
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
                Title = $"Карта розбіжностей: {work.Title}",
                Differences = adaptationDto.Differences.Select(d => new Domain.Entities.Difference
                {
                    Id = Guid.NewGuid(),
                    Title = d.Title,
                    BookText = d.BookText,
                    FilmText = d.FilmText,
                    IsSpoiler = d.IsSpoiler,
                    ImportanceLevel = d.ImportanceLevel ?? (d.IsSpoiler ? "high" : "medium"),
                }).ToList(),
            };
            work.DifferenceMap = diffMap;
            await this.context.DifferenceMaps.AddAsync(diffMap);
        }

        await this.context.Books.AddAsync(book);
        await this.context.Adaptations.AddAsync(adaptation);
        await this.context.Works.AddAsync(work);

        await this.context.SaveChangesAsync();

        var result = this.mapper.Map<AdaptationDto>(adaptation);
        result.Author = adaptationDto.Author;
        result.Genre = adaptationDto.Genre;
        return result;
    }

    /// <inheritdoc/>
    public async Task<AdaptationDto?> UpdateAdaptationAsync(Guid id, AdaptationDto adaptationDto)
    {
        var adaptation = await this.context.Adaptations
            .Include(a => a.Work)
                .ThenInclude(w => w!.Book)
                    .ThenInclude(b => b.Authors)
            .Include(a => a.Work)
                .ThenInclude(w => w!.Rating)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (adaptation == null)
        {
            throw new KeyNotFoundException($"Adaptation with ID {id} not found.");
        }

        // Manual mapping to ensure IDs and relations are preserved
        adaptation.Title = adaptationDto.Title;
        adaptation.Type = adaptationDto.Type;
        adaptation.Description = adaptationDto.Description;
        adaptation.ReleaseYear = adaptationDto.ReleaseYear;
        adaptation.PosterUrl = adaptationDto.PosterUrl;
        adaptation.Country = adaptationDto.Country;
        adaptation.Studio = adaptationDto.Studio;

        // Оновлюємо пов'язану книгу та автора
        if (adaptation.Work != null)
        {
            adaptation.Work.Title = adaptationDto.Title;
            adaptation.Work.Summary = adaptationDto.Description;

            if (adaptation.Work.Book != null)
            {
                adaptation.Work.Book.Title = adaptationDto.Title;
                adaptation.Work.Book.Description = adaptationDto.Description;
                adaptation.Work.Book.Genre = adaptationDto.Genre ?? adaptation.Work.Book.Genre;

                if (!string.IsNullOrEmpty(adaptationDto.Author))
                {
                    var existingAuthor = adaptation.Work.Book.Authors.FirstOrDefault();
                    if (existingAuthor != null)
                    {
                        existingAuthor.FullName = adaptationDto.Author;
                    }
                    else
                    {
                        adaptation.Work.Book.Authors.Add(new Domain.Entities.Author { FullName = adaptationDto.Author });
                    }
                }
            }

            // Оновлюємо рейтинги
            if (adaptation.Work.Rating == null)
            {
                adaptation.Work.Rating = new Domain.Entities.Rating { Id = Guid.NewGuid(), WorkId = adaptation.Work.Id };
                await this.context.Ratings.AddAsync(adaptation.Work.Rating);
            }

            adaptation.Work.Rating.BookRating = (decimal?)adaptationDto.BookRating;
            adaptation.Work.Rating.AdaptationRating = (decimal?)adaptationDto.FilmRating;
        }

        await this.context.SaveChangesAsync();

        var result = this.mapper.Map<AdaptationDto>(adaptation);
        result.Author = adaptationDto.Author;
        result.Genre = adaptationDto.Genre;
        return result;
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
