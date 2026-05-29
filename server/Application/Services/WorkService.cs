// <copyright file="WorkService.cs" company="Team 17">
// Copyright (c) Team 17. All rights reserved.
// </copyright>

namespace Book2Screen.Application.Services;

using Book2Screen.Application.DTOs;
using Book2Screen.Application.Filters;
using Book2Screen.Application.Interfaces;
using Book2Screen.Domain.Entities;
using Book2Screen.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Реалізація сервісу для роботи з творами.
/// </summary>
public class WorkService : IWorkService
{
    private readonly ApplicationDbContext context;
    private readonly IVoteService voteService;

    /// <summary>
    /// Initializes a new instance of the <see cref="WorkService"/> class.
    /// </summary>
    /// <param name="context">Контекст бази даних.</param>
    /// <param name="voteService">Сервіс голосування.</param>
    public WorkService(ApplicationDbContext context, IVoteService voteService)
    {
        this.context = context;
        this.voteService = voteService;
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

            if (filter.OnlyWithMap == true)
            {
                query = query.Where(w => w.DifferenceMap != null);
            }
        }

        var works = await query.ToListAsync();
        var dtos = new List<BookScreenItemDto>();

        foreach (var w in works)
        {
            var dto = this.MapToDto(w);
            dto.VoteStats = await this.voteService.GetVoteStatsAsync(w.Id);
            dtos.Add(dto);
        }

        return dtos;
    }

    /// <inheritdoc/>
    public async Task<BookScreenItemDto?> GetWorkByIdAsync(Guid id)
    {
        var work = await this.context.Works
            .Include(w => w.Book)
            .Include(w => w.Adaptation)
            .Include(w => w.Reviews)
            .Include(w => w.DifferenceMap)
                .ThenInclude(dm => dm!.Differences)
            .FirstOrDefaultAsync(w => w.Id == id);

        if (work == null)
        {
            throw new KeyNotFoundException($"Work with ID {id} not found.");
        }

        var dto = this.MapToDto(work);
        dto.VoteStats = await this.voteService.GetVoteStatsAsync(work.Id);

        if (work.DifferenceMap != null)
        {
            dto.HasMap = true;
            dto.Differences = work.DifferenceMap.Differences.Select(d => new DifferenceDto
            {
                Id = d.Id,
                Title = d.DifferenceType,
                BookText = d.Description,
                FilmText = d.Description,
                IsSpoiler = d.ImportanceLevel == "high",
            }).ToList();
        }

        return dto;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<BookScreenItemDto>> GetTopWorksAsync(int count)
    {
        var works = await this.context.Works
            .Include(w => w.Book)
            .Include(w => w.Adaptation)
            .Include(w => w.Reviews)
            .Include(w => w.DifferenceMap)
            .ToListAsync();

        var dtos = new List<BookScreenItemDto>();
        foreach (var w in works)
        {
            var dto = this.MapToDto(w);
            dto.VoteStats = await this.voteService.GetVoteStatsAsync(w.Id);
            dto.HasMap = w.DifferenceMap != null;
            dtos.Add(dto);
        }

        return dtos.OrderByDescending(d => d.FilmRating).Take(count);
    }

    private BookScreenItemDto MapToDto(Work w)
    {
        return new BookScreenItemDto
        {
            Id = w.Id,
            Title = w.Title,
            Year = w.Adaptation.ReleaseYear ?? 0,
            Genre = w.Book.Genre ?? "Драма",
            Country = w.Adaptation.Country ?? "Unknown",
            Poster = w.Adaptation.PosterUrl ?? "https://via.placeholder.com/300x450",
            BookRating = w.Reviews.Any(r => r.TargetType == "book")
                ? Math.Round(w.Reviews.Where(r => r.TargetType == "book").Average(r => r.Rating), 1)
                : 0,
            FilmRating = w.Reviews.Any(r => r.TargetType == "adaptation")
                ? Math.Round(w.Reviews.Where(r => r.TargetType == "adaptation").Average(r => r.Rating), 1)
                : 0,
            Description = w.Summary ?? w.Book.Description ?? string.Empty,
            FilmYear = w.Adaptation.ReleaseYear,
            FilmCountry = w.Adaptation.Country,
            FilmPoster = w.Adaptation.PosterUrl,
            HasMap = w.DifferenceMap != null,
        };
    }
}
