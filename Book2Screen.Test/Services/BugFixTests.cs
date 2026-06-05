// <copyright file="BugFixTests.cs" company="Team 17">
// Copyright (c) Team 17. All rights reserved.
// </copyright>

namespace Book2Screen.Test.Services;

using Book2Screen.Application.DTOs;
using Book2Screen.Application.Services;
using Book2Screen.Domain.Entities;
using Book2Screen.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

/// <summary>
/// Тести для перевірки виправлення багів BUG-035, BUG-050 та MH-010.
/// </summary>
public class BugFixTests : IDisposable
{
    private readonly ApplicationDbContext context;
    private readonly ReviewService reviewService;
    private readonly UserService userService;

    public BugFixTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        this.context = new ApplicationDbContext(options);
        this.reviewService = new ReviewService(this.context);
        this.userService = new UserService(this.context);
    }

    /// <summary>
    /// Валідація BUG-035: Перевірка Upsert-логіки для рейтингів (оновлення замість дублювання).
    /// </summary>
    [Fact]
    public async Task AddReviewAsync_UpsertLogic_ShouldUpdateExistingRating()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var workId = Guid.NewGuid();
        await this.context.Users.AddAsync(new User { Id = userId, Username = "testuser", Email = "test@test.com", PasswordHash = "hash" });
        await this.context.Works.AddAsync(new Work { Id = workId, Title = "Test Work" });
        await this.context.SaveChangesAsync();

        var firstRequest = new ReviewRequest
        {
            WorkId = workId,
            Rating = 8.0,
            TargetType = "book",
            Text = "First text",
            IsSpoiler = false
        };

        var secondRequest = new ReviewRequest
        {
            WorkId = workId,
            Rating = 9.5,
            TargetType = "book",
            Text = null, // Тільки рейтинг
            IsSpoiler = true
        };

        // Act
        await this.reviewService.AddReviewAsync(userId, firstRequest);
        await this.reviewService.AddReviewAsync(userId, secondRequest);

        // Assert
        var reviews = await this.context.Reviews.Where(r => r.UserId == userId && r.WorkId == workId).ToListAsync();
        Assert.Single(reviews); // Повинна бути лише одна запис
        Assert.Equal(9.5, reviews[0].Rating);
        Assert.Equal("First text", reviews[0].Text); // Текст зберігся від першого запиту
        Assert.True(reviews[0].IsSpoiler);
    }

    /// <summary>
    /// Валідація MH-010: Оновлення аватара користувача.
    /// </summary>
    [Fact]
    public async Task UpdateAvatarAsync_ShouldUpdateUserAvatarUrl()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, Username = "avataruser", Email = "avatar@test.com", PasswordHash = "hash", AvatarUrl = "old.jpg" };
        await this.context.Users.AddAsync(user);
        await this.context.SaveChangesAsync();

        // Act
        await this.userService.UpdateAvatarAsync(userId, "new-avatar.png");

        // Assert
        var updatedUser = await this.context.Users.FindAsync(userId);
        Assert.Equal("new-avatar.png", updatedUser?.AvatarUrl);
    }

    public void Dispose()
    {
        this.context.Database.EnsureDeleted();
        this.context.Dispose();
    }
}
