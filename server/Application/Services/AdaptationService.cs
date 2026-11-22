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

        // Ручне мапування полів
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

        var work = new Domain.Entities.Work
        {
            Id = Guid.NewGuid(),
            Title = adaptationDto.Title,
            BookId = book.Id,
            AdaptationId = adaptation.Id,
            Summary = adaptationDto.Description,
            CreatedAt = DateTime.UtcNow,
        };
        adaptation.Work = work;

        var rating = new Domain.Entities.Rating
        {
            Id = Guid.NewGuid(),
            WorkId = work.Id,
            BookRating = (decimal?)(adaptationDto.BookRating ?? 0),
            AdaptationRating = (decimal?)(adaptationDto.FilmRating ?? 0),
            VotesCount = 1,
            CreatedAt = DateTime.UtcNow,
        };
        work.Rating = rating;
        await this.context.Ratings.AddAsync(rating);

        if (adaptationDto.Differences != null && adaptationDto.Differences.Any())
        {
            var mapId = Guid.NewGuid();
            var diffMap = new Domain.Entities.DifferenceMap
            {
                Id = mapId,
                WorkId = work.Id,
                Title = $"Карта розбіжностей: {work.Title}",
                CreatedAt = DateTime.UtcNow,
                Version = 1,
            };

            diffMap.Differences = adaptationDto.Differences.Select(d => new Domain.Entities.Difference
            {
                Id = Guid.NewGuid(),
                MapId = mapId,
                Title = d.Title,
                BookText = d.BookText,
                FilmText = d.FilmText,
                IsSpoiler = d.IsSpoiler,
                ImportanceLevel = d.ImportanceLevel ?? (d.IsSpoiler ? "high" : "medium"),
                CreatedAt = DateTime.UtcNow,
            }).ToList();

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
        if (work.DifferenceMap != null)
        {
            result.Differences = work.DifferenceMap.Differences.Select(d => new DifferenceDto
            {
                Id = d.Id,
                Title = d.Title,
                BookText = d.BookText,
                FilmText = d.FilmText,
                IsSpoiler = d.IsSpoiler,
                ImportanceLevel = d.ImportanceLevel,
            }).ToList();
        }

        return result;
    }

    /// <inheritdoc/>
    public async Task<BookScreenItemDto?> UpdateAdaptationAsync(Guid id, AdaptationDto adaptationDto)
    {
        // NO SPLIT QUERY for update to avoid tracking inconsistencies
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

        // 1. Update Adaptation
        adaptation.Title = adaptationDto.Title;
        adaptation.Type = adaptationDto.Type;
        adaptation.Description = adaptationDto.Description;
        adaptation.ReleaseYear = adaptationDto.ReleaseYear;
        adaptation.PosterUrl = adaptationDto.PosterUrl;
        adaptation.Country = adaptationDto.Country;
        adaptation.Studio = adaptationDto.Studio;
        adaptation.UpdatedAt = DateTime.UtcNow;

        if (adaptation.Work != null)
        {
            // 2. Update Work
            adaptation.Work.Title = adaptationDto.Title;
            adaptation.Work.Summary = adaptationDto.Description;
            adaptation.Work.UpdatedAt = DateTime.UtcNow;

            if (adaptation.Work.Book != null)
            {
                // 3. Update Book
                var book = adaptation.Work.Book;
                book.Title = string.IsNullOrEmpty(adaptationDto.BookTitle) ? adaptationDto.Title : adaptationDto.BookTitle;
                book.Description = string.IsNullOrEmpty(adaptationDto.BookDescription) ? adaptationDto.Description : adaptationDto.BookDescription;
                book.Genre = adaptationDto.Genre ?? book.Genre;
                book.PublicationYear = adaptationDto.BookYear ?? book.PublicationYear;
                book.CoverImageUrl = adaptationDto.BookPoster ?? book.CoverImageUrl;
                book.UpdatedAt = DateTime.UtcNow;

                // Sync Author
                if (!string.IsNullOrEmpty(adaptationDto.Author) && !book.Authors.Any(a => a.FullName == adaptationDto.Author))
                {
                    book.Authors.Clear();
                    var author = await this.context.Authors.FirstOrDefaultAsync(a => a.FullName == adaptationDto.Author);
                    if (author == null)
                    {
                        author = new Domain.Entities.Author { Id = Guid.NewGuid(), FullName = adaptationDto.Author };
                        await this.context.Authors.AddAsync(author);
                    }

                    book.Authors.Add(author);
                }
            }

            // 4. Update Rating
            if (adaptation.Work.Rating == null)
            {
                adaptation.Work.Rating = new Domain.Entities.Rating { Id = Guid.NewGuid(), WorkId = adaptation.Work.Id };
                await this.context.Ratings.AddAsync(adaptation.Work.Rating);
            }

            adaptation.Work.Rating.BookRating = (decimal?)(adaptationDto.BookRating ?? 0);
            adaptation.Work.Rating.AdaptationRating = (decimal?)(adaptationDto.FilmRating ?? 0);
            adaptation.Work.Rating.UpdatedAt = DateTime.UtcNow;

            // 5. Update Map
            if (adaptationDto.Differences != null)
            {
                if (adaptation.Work.DifferenceMap == null)
                {
                    // Double check if it exists in DB but wasn't loaded
                    var existingMap = await this.context.DifferenceMaps
                        .Include(m => m.Differences)
                        .FirstOrDefaultAsync(m => m.WorkId == adaptation.Work.Id);

                    if (existingMap != null)
                    {
                        adaptation.Work.DifferenceMap = existingMap;
                    }
                    else
                    {
                        adaptation.Work.DifferenceMap = new Domain.Entities.DifferenceMap
                        {
                            Id = Guid.NewGuid(),
                            WorkId = adaptation.Work.Id,
                            Title = $"Карта розбіжностей: {adaptation.Work.Title}",
                            Version = 1,
                        };
                        await this.context.DifferenceMaps.AddAsync(adaptation.Work.DifferenceMap);
                    }
                }

                var map = adaptation.Work.DifferenceMap;
                map.Title = $"Карта розбіжностей: {adaptation.Work.Title}";
                map.UpdatedAt = DateTime.UtcNow;

                var incomingIds = adaptationDto.Differences.Where(d => d.Id.HasValue).Select(d => d.Id!.Value).ToHashSet();

                // Remove obsolete
                var currentDiffs = map.Differences.ToList();
                foreach (var diff in currentDiffs.Where(diff => !incomingIds.Contains(diff.Id)))
                {
                    this.context.Differences.Remove(diff);
                }

                // Update or Add
                foreach (var dto in adaptationDto.Differences)
                {
                    if (dto.Id.HasValue)
                    {
                        var existing = map.Differences.FirstOrDefault(d => d.Id == dto.Id.Value);
                        if (existing != null)
                        {
                            existing.Title = dto.Title;
                            existing.BookText = dto.BookText;
                            existing.FilmText = dto.FilmText;
                            existing.IsSpoiler = dto.IsSpoiler;
                            existing.ImportanceLevel = dto.ImportanceLevel ?? (dto.IsSpoiler ? "high" : "medium");
                            existing.UpdatedAt = DateTime.UtcNow;
                        }
                        else
                        {
                            map.Differences.Add(new Domain.Entities.Difference
                            {
                                Id = Guid.NewGuid(),
                                MapId = map.Id,
                                Title = dto.Title,
                                BookText = dto.BookText,
                                FilmText = dto.FilmText,
                                IsSpoiler = dto.IsSpoiler,
                                ImportanceLevel = dto.ImportanceLevel ?? (dto.IsSpoiler ? "high" : "medium"),
                            });
                        }
                    }
                    else
                    {
                        map.Differences.Add(new Domain.Entities.Difference
                        {
                            Id = Guid.NewGuid(),
                            MapId = map.Id,
                            Title = dto.Title,
                            BookText = dto.BookText,
                            FilmText = dto.FilmText,
                            IsSpoiler = dto.IsSpoiler,
                            ImportanceLevel = dto.ImportanceLevel ?? (dto.IsSpoiler ? "high" : "medium"),
                        });
                    }
                }
            }
        }

        await this.context.SaveChangesAsync();

        // Reload fresh graph for response
        if (adaptation.Work == null)
        {
            return null;
        }

        var finalWork = await this.context.Works
            .Include(w => w.Book).ThenInclude(b => b!.Authors)
            .Include(w => w.Adaptation)
            .Include(w => w.Rating)
            .Include(w => w.DifferenceMap).ThenInclude(dm => dm!.Differences)
            .FirstOrDefaultAsync(w => w.Id == adaptation.Work!.Id);

        return this.mapper.Map<BookScreenItemDto>(finalWork);
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
