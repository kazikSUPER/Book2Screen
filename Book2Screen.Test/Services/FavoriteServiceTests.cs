// <copyright file="FavoriteServiceTests.cs" company="Team 17">
// Copyright (c) Team 17. All rights reserved.
// </copyright>

namespace Book2Screen.Tests.Services;

using AutoMapper;
using Book2Screen.Application.Services;
using Book2Screen.Domain.Exceptions;
using Book2Screen.Tests.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

/// <summary>
/// Юніт тести для FavoriteService.
/// </summary>
public class FavoriteServiceTests
{
    private readonly Mock<IMapper> mapperMock = new();

    // ── AddToFavoritesAsync ───────────────────────────────────────────────────

    /// <summary>
    /// Додавання нового запису у вибране зберігається в БД.
    /// </summary>
    [Fact]
    public async Task AddToFavoritesAsync_NewFavorite_SavesToDB()
    {
        using var context = TestDbHelper.CreateContext(nameof(this.AddToFavoritesAsync_NewFavorite_SavesToDB));
        var (userId, workId) = await TestDbHelper.SeedUserAndWork(context);
        var service = new FavoriteService(context, this.mapperMock.Object);

        var result = await service.AddToFavoritesAsync(userId, workId, "read");

        result.Should().BeTrue();
        var saved = await context.Favorites.FirstOrDefaultAsync(f =>
            f.UserId == userId && f.WorkId == workId && f.Kind == "read");
        saved.Should().NotBeNull();
    }

    /// <summary>
    /// Повторне додавання того самого запису кидає ConflictException. (BUG-033 Fix)
    /// </summary>
    [Fact]
    public async Task AddToFavoritesAsync_Duplicate_ThrowsConflictException()
    {
        using var context = TestDbHelper.CreateContext(nameof(this.AddToFavoritesAsync_Duplicate_ThrowsConflictException));
        var (userId, workId) = await TestDbHelper.SeedUserAndWork(context);
        var service = new FavoriteService(context, this.mapperMock.Object);
        await service.AddToFavoritesAsync(userId, workId, "read");

        // Act & Assert
        await service.Invoking(s => s.AddToFavoritesAsync(userId, workId, "read"))
            .Should().ThrowAsync<ConflictException>();
    }

    /// <summary>
    /// Додавання до неіснуючого твору кидає KeyNotFoundException.
    /// </summary>
    [Fact]
    public async Task AddToFavoritesAsync_NonExistentWork_ThrowsKeyNotFoundException()
    {
        using var context = TestDbHelper.CreateContext(nameof(this.AddToFavoritesAsync_NonExistentWork_ThrowsKeyNotFoundException));
        var service = new FavoriteService(context, this.mapperMock.Object);

        await service.Invoking(s => s.AddToFavoritesAsync(Guid.NewGuid(), Guid.NewGuid(), "read"))
            .Should().ThrowAsync<KeyNotFoundException>();
    }

    /// <summary>
    /// Однаковий твір з різним kind (read і watch) — два окремих записи.
    /// </summary>
    [Fact]
    public async Task AddToFavoritesAsync_DifferentKind_AddsBothEntries()
    {
        using var context = TestDbHelper.CreateContext(nameof(this.AddToFavoritesAsync_DifferentKind_AddsBothEntries));
        var (userId, workId) = await TestDbHelper.SeedUserAndWork(context);
        var service = new FavoriteService(context, this.mapperMock.Object);

        await service.AddToFavoritesAsync(userId, workId, "read");
        await service.AddToFavoritesAsync(userId, workId, "watch");

        var count = await context.Favorites.CountAsync(f => f.UserId == userId && f.WorkId == workId);
        count.Should().Be(2);
    }

    // ── RemoveFromFavoritesAsync ──────────────────────────────────────────────

    /// <summary>
    /// Видалення існуючого запису з вибраного.
    /// </summary>
    [Fact]
    public async Task RemoveFromFavoritesAsync_ExistingFavorite_RemovesFromDB()
    {
        using var context = TestDbHelper.CreateContext(nameof(this.RemoveFromFavoritesAsync_ExistingFavorite_RemovesFromDB));
        var (userId, workId) = await TestDbHelper.SeedUserAndWork(context);
        var service = new FavoriteService(context, this.mapperMock.Object);
        await service.AddToFavoritesAsync(userId, workId, "read");

        var result = await service.RemoveFromFavoritesAsync(userId, workId, "read");

        result.Should().BeTrue();
        var exists = await context.Favorites.AnyAsync(f => f.UserId == userId && f.WorkId == workId);
        exists.Should().BeFalse();
    }

    /// <summary>
    /// Видалення неіснуючого запису повертає true (ідемпотентність).
    /// </summary>
    [Fact]
    public async Task RemoveFromFavoritesAsync_NonExistent_ReturnsTrueIdempotent()
    {
        using var context = TestDbHelper.CreateContext(nameof(this.RemoveFromFavoritesAsync_NonExistent_ReturnsTrueIdempotent));
        var (userId, workId) = await TestDbHelper.SeedUserAndWork(context);
        var service = new FavoriteService(context, this.mapperMock.Object);

        var result = await service.RemoveFromFavoritesAsync(userId, workId, "read");

        result.Should().BeTrue();
    }

    // ── IsFavoriteAsync ───────────────────────────────────────────────────────

    /// <summary>
    /// IsFavorite повертає true якщо запис є в БД.
    /// </summary>
    [Fact]
    public async Task IsFavoriteAsync_ExistingFavorite_ReturnsTrue()
    {
        using var context = TestDbHelper.CreateContext(nameof(this.IsFavoriteAsync_ExistingFavorite_ReturnsTrue));
        var (userId, workId) = await TestDbHelper.SeedUserAndWork(context);
        var service = new FavoriteService(context, this.mapperMock.Object);
        await service.AddToFavoritesAsync(userId, workId, "read");

        var result = await service.IsFavoriteAsync(userId, workId, "read");

        result.Should().BeTrue();
    }

    /// <summary>
    /// IsFavorite повертає false якщо запис відсутній.
    /// </summary>
    [Fact]
    public async Task IsFavoriteAsync_NonExistent_ReturnsFalse()
    {
        using var context = TestDbHelper.CreateContext(nameof(this.IsFavoriteAsync_NonExistent_ReturnsFalse));
        var (userId, workId) = await TestDbHelper.SeedUserAndWork(context);
        var service = new FavoriteService(context, this.mapperMock.Object);

        var result = await service.IsFavoriteAsync(userId, workId, "read");

        result.Should().BeFalse();
    }
}
