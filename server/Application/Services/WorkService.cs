// <copyright file="WorkService.cs" company="Team 17">
// Copyright (c) Team 17. All rights reserved.
// </copyright>

namespace Book2Screen.Application.Services;

using Book2Screen.Application.DTOs;
using Book2Screen.Application.Filters;
using Book2Screen.Application.Interfaces;
using Book2Screen.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Реалізація сервісу для роботи з творами.
/// </summary>
public class WorkService : IWorkService
{
    private readonly ApplicationDbContext context;

    /// <summary>
    /// Initializes a new instance of the <see cref="WorkService"/> class.
    /// </summary>
    /// <param name="context">Контекст бази даних.</param>
    public WorkService(ApplicationDbContext context)
    {
        this.context = context;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<BookScreenItemDto>> GetAllWorksAsync(WorkFilter? filter = null)
    {
        var query = this.context.Works
            .Include(w => w.Book)
            .Include(w => w.Adaptation)
            .Include(w => w.Reviews)
            .AsQueryable();

        if (filter != null)
        {
            if (!string.IsNullOrEmpty(filter.Search))
            {
                query = query.Where(w => w.Title.ToLower().Contains(filter.Search.ToLower()));
            }

            if (!string.IsNullOrEmpty(filter.Genre))
            {
                query = query.Where(w => w.Book.Genre != null && w.Book.Genre.ToLower() == filter.Genre.ToLower());
            }

            if (!string.IsNullOrEmpty(filter.Country))
            {
                query = query.Where(w => w.Adaptation.Country != null && w.Adaptation.Country.ToLower() == filter.Country.ToLower());
            }
        }

        return await query
            .Select(w => new BookScreenItemDto
            {
                Id = w.Id,
                Title = w.Title,
                Year = w.Adaptation.ReleaseYear ?? 0,
                Genre = w.Book.Genre ?? "Драма",
                Country = w.Adaptation.Country ?? "Unknown",
                Poster = w.Adaptation.PosterUrl ?? "https://via.placeholder.com/300x450",
                BookRating = w.Reviews.Any(r => r.TargetType == "book")
                    ? w.Reviews.Where(r => r.TargetType == "book").Average(r => r.Rating)
                    : 0,
                FilmRating = w.Reviews.Any(r => r.TargetType == "adaptation")
                    ? w.Reviews.Where(r => r.TargetType == "adaptation").Average(r => r.Rating)
                    : 0,
                Description = w.Summary ?? w.Book.Description ?? string.Empty,
            })
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<BookScreenItemDto?> GetWorkByIdAsync(Guid id)
    {
        return await this.context.Works
            .Include(w => w.Book)
            .Include(w => w.Adaptation)
            .Include(w => w.Reviews)
            .Where(w => w.Id == id)
            .Select(w => new BookScreenItemDto
            {
                Id = w.Id,
                Title = w.Title,
                Year = w.Adaptation.ReleaseYear ?? 0,
                Genre = w.Book.Genre ?? "Драма",
                Country = w.Adaptation.Country ?? "Unknown",
                Poster = w.Adaptation.PosterUrl ?? "https://via.placeholder.com/300x450",
                BookRating = w.Reviews.Any(r => r.TargetType == "book")
                    ? w.Reviews.Where(r => r.TargetType == "book").Average(r => r.Rating)
                    : 0,
                FilmRating = w.Reviews.Any(r => r.TargetType == "adaptation")
                    ? w.Reviews.Where(r => r.TargetType == "adaptation").Average(r => r.Rating)
                    : 0,
                Description = w.Summary ?? w.Book.Description ?? string.Empty,
            })
            .FirstOrDefaultAsync();
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<BookScreenItemDto>> GetTopWorksAsync(int count)
    {
        return await this.context.Works
            .Include(w => w.Book)
            .Include(w => w.Adaptation)
            .Include(w => w.Reviews)
            .Select(w => new BookScreenItemDto
            {
                Id = w.Id,
                Title = w.Title,
                Year = w.Adaptation.ReleaseYear ?? 0,
                Genre = w.Book.Genre ?? "Драма",
                Country = w.Adaptation.Country ?? "Unknown",
                Poster = w.Adaptation.PosterUrl ?? "https://via.placeholder.com/300x450",
                BookRating = w.Reviews.Any(r => r.TargetType == "book")
                    ? w.Reviews.Where(r => r.TargetType == "book").Average(r => r.Rating)
                    : 0,
                FilmRating = w.Reviews.Any(r => r.TargetType == "adaptation")
                    ? w.Reviews.Where(r => r.TargetType == "adaptation").Average(r => r.Rating)
                    : 0,
                Description = w.Summary ?? w.Book.Description ?? string.Empty,
            })
            .OrderByDescending(w => w.FilmRating)
            .Take(count)
            .ToListAsync();
    }
}
