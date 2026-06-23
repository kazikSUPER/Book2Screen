// <copyright file="FavoriteService.cs" company="Team 17">
// Copyright (c) Team 17. All rights reserved.
// </copyright>

namespace Book2Screen.Application.Services;

using AutoMapper;
using Book2Screen.Application.DTOs;
using Book2Screen.Application.Interfaces;
using Book2Screen.Domain.Entities;
using Book2Screen.Domain.Exceptions;
using Book2Screen.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Сервіс для керування списком обраного.
/// </summary>
public class FavoriteService : IFavoriteService
{
    private readonly ApplicationDbContext context;
    private readonly IMapper mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="FavoriteService"/> class.
    /// </summary>
    /// <param name="context">Контекст БД.</param>
    /// <param name="mapper">Маппер.</param>
    public FavoriteService(ApplicationDbContext context, IMapper mapper)
    {
        this.context = context;
        this.mapper = mapper;
    }

    /// <inheritdoc/>
    public async Task<bool> AddToFavoritesAsync(Guid userId, Guid workId, string kind = "favorite")
    {
        var workExists = await this.context.Works.AnyAsync(w => w.Id == workId);
        if (!workExists)
        {
            throw new KeyNotFoundException($"Work with ID {workId} not found.");
        }

        var alreadyFavorite = await this.context.Favorites
            .AnyAsync(f => f.UserId == userId && f.WorkId == workId && f.Kind == kind);

        if (alreadyFavorite)
        {
            throw new ConflictException($"Work with ID {workId} is already in favorites with kind '{kind}'.");
        }

        var favorite = new Favorite
        {
            UserId = userId,
            WorkId = workId,
            Kind = kind,
        };

        await this.context.Favorites.AddAsync(favorite);
        return await this.context.SaveChangesAsync() > 0;
    }

    /// <inheritdoc/>
    public async Task<bool> RemoveFromFavoritesAsync(Guid userId, Guid workId, string? kind = null)
    {
        var query = this.context.Favorites
            .Where(f => f.UserId == userId && f.WorkId == workId);

        if (!string.IsNullOrEmpty(kind))
        {
            query = query.Where(f => f.Kind == kind);
        }

        var favorites = await query.ToListAsync();

        if (favorites.Count == 0)
        {
            return true;
        }

        this.context.Favorites.RemoveRange(favorites);
        return await this.context.SaveChangesAsync() > 0;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<BookScreenItemDto>> GetUserFavoritesAsync(Guid userId, string? kind = null)
    {
        var query = this.context.Favorites
            .Where(f => f.UserId == userId);

        if (!string.IsNullOrEmpty(kind))
        {
            query = query.Where(f => f.Kind == kind);
        }

        var works = await query
            .Include(f => f.Work)
                .ThenInclude(w => w.Book)
                    .ThenInclude(b => b.Authors)
            .Include(f => f.Work)
                .ThenInclude(w => w.Adaptation)
            .Select(f => f.Work)
            .ToListAsync();

        return this.mapper.Map<IEnumerable<BookScreenItemDto>>(works);
    }

    /// <inheritdoc/>
    public async Task<bool> IsFavoriteAsync(Guid userId, Guid workId, string? kind = null)
    {
        var query = this.context.Favorites
            .Where(f => f.UserId == userId && f.WorkId == workId);

        if (!string.IsNullOrEmpty(kind))
        {
            query = query.Where(f => f.Kind == kind);
        }

        return await query.AnyAsync();
    }
}
