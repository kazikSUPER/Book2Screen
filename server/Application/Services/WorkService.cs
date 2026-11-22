// <copyright file="WorkService.cs" company="Team 17">
// Copyright (c) Team 17. All rights reserved.
// </copyright>

namespace Book2Screen.Application.Services;

using AutoMapper;
using Book2Screen.Application.DTOs;
using Book2Screen.Application.Filters;
using Book2Screen.Application.Interfaces;
using Book2Screen.Domain.Entities;
using Book2Screen.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Сервіс для роботи з творами (Work).
/// Об'єднує книги та їх адаптації.
/// </summary>
public class WorkService : IWorkService
{
    private readonly ApplicationDbContext context;
    private readonly IMapper mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="WorkService"/> class.
    /// </summary>
    /// <param name="context">Контекст бази даних.</param>
    /// <param name="mapper">Мапер об'єктів.</param>
    public WorkService(ApplicationDbContext context, IMapper mapper)
    {
        this.context = context;
        this.mapper = mapper;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<BookScreenItemDto>> GetAllWorksAsync(WorkFilter? filter = null)
    {
        var query = this.context.Works
            .Include(w => w.Book)
                .ThenInclude(b => b.Authors)
            .Include(w => w.Adaptation)
            .Include(w => w.Reviews)
            .Include(w => w.Votes)
            .Include(w => w.Rating)
            .Include(w => w.DifferenceMap)
                .ThenInclude(dm => dm!.Differences)
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
        return works.Select(w => this.mapper.Map<BookScreenItemDto>(w)).ToList();
    }

    /// <inheritdoc/>
    public async Task<BookScreenItemDto?> GetWorkByIdAsync(Guid id)
    {
        var work = await this.context.Works
            .Include(w => w.Book)
                .ThenInclude(b => b.Authors)
            .Include(w => w.Adaptation)
            .Include(w => w.Reviews)
            .Include(w => w.Votes)
            .Include(w => w.Rating)
            .Include(w => w.DifferenceMap)
                .ThenInclude(dm => dm!.Differences)
            .FirstOrDefaultAsync(w => w.Id == id);

        if (work == null)
        {
            throw new KeyNotFoundException($"Work with ID {id} not found.");
        }

        return this.mapper.Map<BookScreenItemDto>(work);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<BookScreenItemDto>> GetTopWorksAsync(int count)
    {
        var works = await this.context.Works
            .Include(w => w.Book)
                .ThenInclude(b => b.Authors)
            .Include(w => w.Adaptation)
            .Include(w => w.Reviews)
            .Include(w => w.Votes)
            .Include(w => w.Rating)
            .Include(w => w.DifferenceMap)
                .ThenInclude(dm => dm!.Differences)
            .ToListAsync();

        return works
            .Select(w => this.mapper.Map<BookScreenItemDto>(w))
            .OrderByDescending(d => d.FilmRating)
            .Take(count)
            .ToList();
    }
}
