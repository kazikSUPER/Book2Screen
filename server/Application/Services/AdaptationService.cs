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
        adaptation.CreatedAt = DateTime.UtcNow;

        // 1. Підготовка автора
        var authors = new List<Domain.Entities.Author>();
        if (!string.IsNullOrEmpty(adaptationDto.Author))
        {
            var author = await this.context.Authors
                .FirstOrDefaultAsync(a => a.FullName == adaptationDto.Author);

            if (author == null)
            {
                author = new Domain.Entities.Author { Id = Guid.NewGuid(), FullName = adaptationDto.Author };
                await this.context.Authors.AddAsync(author);
            }

            authors.Add(author);
        }

        // 2. Створюємо Книгу
        var book = new Domain.Entities.Book
        {
            Id = Guid.NewGuid(),
            Title = string.IsNullOrEmpty(adaptationDto.BookTitle) ? adaptationDto.Title : adaptationDto.BookTitle,
            Description = string.IsNullOrEmpty(adaptationDto.BookDescription) ? adaptationDto.Description : adaptationDto.BookDescription,
            Genre = adaptationDto.Genre ?? "Драма",
            PublicationYear = adaptationDto.BookYear ?? adaptationDto.ReleaseYear ?? 0,
            CoverImageUrl = adaptationDto.BookPoster ?? adaptationDto.PosterUrl,
            Authors = authors,
            CreatedAt = DateTime.UtcNow,
        };

        // 3. Створюємо Твір (Work) та пов'язуємо з Книгою та Адаптацією
        var work = new Domain.Entities.Work
        {
            Id = Guid.NewGuid(),
            Title = adaptationDto.Title,
            BookId = book.Id,
            AdaptationId = adaptation.Id,
            Summary = adaptationDto.Description,
            CreatedAt = DateTime.UtcNow,
        };

        // 3.1. Створюємо сутність Rating
        var rating = new Domain.Entities.Rating
        {
            Id = Guid.NewGuid(),
            WorkId = work.Id,
            BookRating = (decimal?)(adaptationDto.BookRating ?? 0),
            AdaptationRating = (decimal?)(adaptationDto.FilmRating ?? 0),
            VotesCount = 1, // Початковий голос
            CreatedAt = DateTime.UtcNow,
        };
        work.Rating = rating;
        await this.context.Ratings.AddAsync(rating);

        // 4. Якщо передано розбіжності — створюємо карту
        if (adaptationDto.Differences != null && adaptationDto.Differences.Any())
        {
            var diffMap = new Domain.Entities.DifferenceMap
            {
                Id = Guid.NewGuid(),
                WorkId = work.Id,
                Title = $"Карта розбіжностей: {work.Title}",
                CreatedAt = DateTime.UtcNow,
                Differences = adaptationDto.Differences.Select(d => new Domain.Entities.Difference
                {
                    Id = Guid.NewGuid(),
                    Title = d.Title,
                    BookText = d.BookText,
                    FilmText = d.FilmText,
                    IsSpoiler = d.IsSpoiler,
                    ImportanceLevel = d.ImportanceLevel ?? (d.IsSpoiler ? "high" : "medium"),
                    CreatedAt = DateTime.UtcNow,
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
        result.BookTitle = book.Title;
        result.BookDescription = book.Description;
        result.BookYear = book.PublicationYear;
        result.BookPoster = book.CoverImageUrl;
        return result;
    }

    /// <inheritdoc/>
    public async Task<BookScreenItemDto?> UpdateAdaptationAsync(Guid id, AdaptationDto adaptationDto)
    {
        var adaptation = await this.context.Adaptations
            .Include(a => a.Work)
                .ThenInclude(w => w!.Book)
                    .ThenInclude(b => b.Authors)
            .Include(a => a.Work)
                .ThenInclude(w => w!.Rating)
            .Include(a => a.Work)
                .ThenInclude(w => w!.DifferenceMap)
                    .ThenInclude(dm => dm!.Differences)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (adaptation == null)
        {
            throw new KeyNotFoundException($"Adaptation with ID {id} not found.");
        }

        // 1. Оновлюємо основні поля адаптації
        adaptation.Title = adaptationDto.Title;
        adaptation.Type = adaptationDto.Type;
        adaptation.Description = adaptationDto.Description;
        adaptation.ReleaseYear = adaptationDto.ReleaseYear;
        adaptation.PosterUrl = adaptationDto.PosterUrl;
        adaptation.Country = adaptationDto.Country;
        adaptation.Studio = adaptationDto.Studio;
        adaptation.UpdatedAt = DateTime.UtcNow;

        // 2. Оновлюємо пов'язаний твір та книгу
        if (adaptation.Work != null)
        {
            adaptation.Work.Title = adaptationDto.Title;
            adaptation.Work.Summary = adaptationDto.Description;
            adaptation.Work.UpdatedAt = DateTime.UtcNow;

            if (adaptation.Work.Book != null)
            {
                adaptation.Work.Book.Title = string.IsNullOrEmpty(adaptationDto.BookTitle) ? adaptationDto.Title : adaptationDto.BookTitle;
                adaptation.Work.Book.Description = string.IsNullOrEmpty(adaptationDto.BookDescription) ? adaptationDto.Description : adaptationDto.BookDescription;
                adaptation.Work.Book.Genre = adaptationDto.Genre ?? adaptation.Work.Book.Genre;
                adaptation.Work.Book.PublicationYear = adaptationDto.BookYear ?? adaptation.Work.Book.PublicationYear;
                adaptation.Work.Book.CoverImageUrl = adaptationDto.BookPoster ?? adaptation.Work.Book.CoverImageUrl;
                adaptation.Work.Book.UpdatedAt = DateTime.UtcNow;

                // 3. Синхронізація автора (MVP: підтримуємо одного основного автора)
                if (!string.IsNullOrEmpty(adaptationDto.Author))
                {
                    var existingAuthors = adaptation.Work.Book.Authors.ToList();
                    if (!existingAuthors.Any(a => a.FullName == adaptationDto.Author))
                    {
                        // Прибираємо старі зв'язки
                        adaptation.Work.Book.Authors.Clear();

                        // Шукаємо автора в БД або створюємо нового
                        var author = await this.context.Authors
                            .FirstOrDefaultAsync(a => a.FullName == adaptationDto.Author);

                        if (author == null)
                        {
                            author = new Domain.Entities.Author
                            {
                                Id = Guid.NewGuid(),
                                FullName = adaptationDto.Author,
                                CreatedAt = DateTime.UtcNow,
                            };
                            await this.context.Authors.AddAsync(author);
                        }

                        adaptation.Work.Book.Authors.Add(author);
                    }
                }
            }

            // 4. Оновлення рейтингів
            if (adaptation.Work.Rating == null)
            {
                adaptation.Work.Rating = new Domain.Entities.Rating
                {
                    Id = Guid.NewGuid(),
                    WorkId = adaptation.Work.Id,
                    CreatedAt = DateTime.UtcNow,
                };
                await this.context.Ratings.AddAsync(adaptation.Work.Rating);
            }

            adaptation.Work.Rating.BookRating = (decimal?)(adaptationDto.BookRating ?? 0);
            adaptation.Work.Rating.AdaptationRating = (decimal?)(adaptationDto.FilmRating ?? 0);
            adaptation.Work.Rating.UpdatedAt = DateTime.UtcNow;

            // 5. Оновлення карти розбіжностей (Differences)
            if (adaptationDto.Differences != null)
            {
                if (adaptation.Work.DifferenceMap == null)
                {
                    adaptation.Work.DifferenceMap = new Domain.Entities.DifferenceMap
                    {
                        Id = Guid.NewGuid(),
                        WorkId = adaptation.Work.Id,
                        Title = $"Карта розбіжностей: {adaptation.Work.Title}",
                        CreatedAt = DateTime.UtcNow,
                    };
                    await this.context.DifferenceMaps.AddAsync(adaptation.Work.DifferenceMap);
                }

                // Видаляємо старі відмінності, які не передані
                var incomingIds = adaptationDto.Differences.Where(d => d.Id.HasValue).Select(d => d.Id!.Value).ToList();
                var obsoleteDiffs = adaptation.Work.DifferenceMap.Differences
                    .Where(d => !incomingIds.Contains(d.Id))
                    .ToList();
                foreach (var oldDiff in obsoleteDiffs)
                {
                    this.context.Differences.Remove(oldDiff);
                    adaptation.Work.DifferenceMap.Differences.Remove(oldDiff);
                }

                // Оновлюємо або додаємо нові відмінності
                foreach (var diffDto in adaptationDto.Differences)
                {
                    if (diffDto.Id.HasValue)
                    {
                        var existingDiff = adaptation.Work.DifferenceMap.Differences
                            .FirstOrDefault(d => d.Id == diffDto.Id.Value);
                        if (existingDiff != null)
                        {
                            existingDiff.Title = diffDto.Title;
                            existingDiff.BookText = diffDto.BookText;
                            existingDiff.FilmText = diffDto.FilmText;
                            existingDiff.IsSpoiler = diffDto.IsSpoiler;
                            existingDiff.ImportanceLevel = diffDto.ImportanceLevel ?? (diffDto.IsSpoiler ? "high" : "medium");
                            existingDiff.UpdatedAt = DateTime.UtcNow;
                        }
                    }
                    else
                    {
                        var newDiff = new Domain.Entities.Difference
                        {
                            Id = Guid.NewGuid(),
                            MapId = adaptation.Work.DifferenceMap.Id,
                            Title = diffDto.Title,
                            BookText = diffDto.BookText,
                            FilmText = diffDto.FilmText,
                            IsSpoiler = diffDto.IsSpoiler,
                            ImportanceLevel = diffDto.ImportanceLevel ?? (diffDto.IsSpoiler ? "high" : "medium"),
                            CreatedAt = DateTime.UtcNow,
                        };
                        adaptation.Work.DifferenceMap.Differences.Add(newDiff);
                    }
                }
            }
        }

        await this.context.SaveChangesAsync();

        // Повертаємо BookScreenItemDto для синхронізації фронтенду
        return this.mapper.Map<BookScreenItemDto>(adaptation.Work);
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
