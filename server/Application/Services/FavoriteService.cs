// <copyright file="FavoriteService.cs" company="Team 17">
// Copyright (c) Team 17. All rights reserved.
// </copyright>

namespace Book2Screen.Application.Services;

using AutoMapper;
using Book2Screen.Application.DTOs;
using Book2Screen.Application.Interfaces;
using Book2Screen.Domain.Entities;
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
    public async Task<bool> AddToFavoritesAsync(Guid userId, Guid workId)
    {
        var alreadyFavorite = await this.context.Favorites
            .AnyAsync(f => f.UserId == userId && f.WorkId == workId);

        if (alreadyFavorite)
        {
            return true;
        }

        var favorite = new Favorite
        {
            UserId = userId,
            WorkId = workId,
        };

        await this.context.Favorites.AddAsync(favorite);
        return await this.context.SaveChangesAsync() > 0;
    }

    /// <inheritdoc/>
    public async Task<bool> RemoveFromFavoritesAsync(Guid userId, Guid workId)
    {
        var favorite = await this.context.Favorites
            .FirstOrDefaultAsync(f => f.UserId == userId && f.WorkId == workId);

        if (favorite == null)
        {
            return true;
        }

        this.context.Favorites.Remove(favorite);
        return await this.context.SaveChangesAsync() > 0;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<BookScreenItemDto>> GetUserFavoritesAsync(Guid userId)
    {
        var works = await this.context.Favorites
            .Where(f => f.UserId == userId)
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
    public async Task<bool> IsFavoriteAsync(Guid userId, Guid workId)
    {
        return await this.context.Favorites
            .AnyAsync(f => f.UserId == userId && f.WorkId == workId);
    }
}
