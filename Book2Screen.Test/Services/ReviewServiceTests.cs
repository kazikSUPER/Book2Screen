// <copyright file="ReviewServiceTests.cs" company="Team 17">
// Copyright (c) Team 17. All rights reserved.
// </copyright>

namespace Book2Screen.Tests.Services;

using Book2Screen.Application.DTOs;
using Book2Screen.Application.Services;
using Book2Screen.Domain.Entities;
using Book2Screen.Domain.Exceptions;
using Book2Screen.Tests.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

/// <summary>
/// Юніт тести для ReviewService.
/// </summary>
public class ReviewServiceTests
{
    private ReviewRequest DefaultRequest(Guid workId, string text = "Текст відгуку довший за десять символів") =>
        new() { WorkId = workId, Text = text, IsSpoiler = false, Rating = 8.0, TargetType = "comparison" };

    // ── AddReviewAsync ────────────────────────────────────────────────────────

    /// <summary>
    /// Додавання відгуку зберігається в БД і повертає ReviewResponse.
    /// </summary>
    [Fact]
    public async Task AddReviewAsync_ValidRequest_SavesAndReturnsResponse()
    {
        using var context = TestDbHelper.CreateContext(nameof(this.AddReviewAsync_ValidRequest_SavesAndReturnsResponse));
        var (userId, workId) = await TestDbHelper.SeedUserAndWork(context);
        var service = new ReviewService(context);

        var result = await service.AddReviewAsync(userId, this.DefaultRequest(workId, "Це чудовий твір що читав!"));

        result.Should().NotBeNull();
        result.UserId.Should().Be(userId);
        result.WorkId.Should().Be(workId);
        result.Rating.Should().Be(8.0);
        result.UserNickname.Should().Be("john_doe"); // BUG-036 verification

        var saved = await context.Reviews.FirstOrDefaultAsync(r => r.UserId == userId);
        saved.Should().NotBeNull();
    }

    /// <summary>
    /// Відгук зі спойлером зберігає IsSpoiler=true. (BUG-028)
    /// </summary>
    [Fact]
    public async Task AddReviewAsync_WithSpoiler_SavesIsSpoilerTrue()
    {
        using var context = TestDbHelper.CreateContext(nameof(this.AddReviewAsync_WithSpoiler_SavesIsSpoilerTrue));
        var (userId, workId) = await TestDbHelper.SeedUserAndWork(context);
        var service = new ReviewService(context);

        var result = await service.AddReviewAsync(userId, new ReviewRequest
        {
            WorkId = workId, Text = "Спойлерний відгук про кінцівку твору тут", IsSpoiler = true, Rating = 8.0, TargetType = "comparison",
        });

        result.IsSpoiler.Should().BeTrue();
        var saved = await context.Reviews.FirstAsync(r => r.UserId == userId);
        saved.IsSpoiler.Should().BeTrue();
    }

    /// <summary>
    /// Відгук до неіснуючого твору кидає KeyNotFoundException.
    /// </summary>
    [Fact]
    public async Task AddReviewAsync_NonExistentWork_ThrowsKeyNotFoundException()
    {
        using var context = TestDbHelper.CreateContext(nameof(this.AddReviewAsync_NonExistentWork_ThrowsKeyNotFoundException));
        var service = new ReviewService(context);

        await service.Invoking(s => s.AddReviewAsync(Guid.NewGuid(), this.DefaultRequest(Guid.NewGuid())))
            .Should().ThrowAsync<KeyNotFoundException>();
    }

    /// <summary>
    /// TargetType конвертується до нижнього регістру.
    /// </summary>
    [Fact]
    public async Task AddReviewAsync_TargetType_SavedAsLowerCase()
    {
        using var context = TestDbHelper.CreateContext(nameof(this.AddReviewAsync_TargetType_SavedAsLowerCase));
        var (userId, workId) = await TestDbHelper.SeedUserAndWork(context);
        var service = new ReviewService(context);

        await service.AddReviewAsync(userId, new ReviewRequest
        {
            WorkId = workId, Text = "Текст відгуку довший за мінімум символів", IsSpoiler = false, Rating = 5.0, TargetType = "COMPARISON",
        });

        var saved = await context.Reviews.FirstAsync(r => r.UserId == userId);
        saved.TargetType.Should().Be("comparison");
    }

    // ── GetReviewsByWorkIdAsync ───────────────────────────────────────────────

    /// <summary>
    /// GetReviewsByWorkId повертає відгуки від усіх юзерів. (BUG-029)
    /// </summary>
    [Fact]
    public async Task GetReviewsByWorkIdAsync_MultipleUsers_ReturnsAllReviews()
    {
        using var context = TestDbHelper.CreateContext(nameof(this.GetReviewsByWorkIdAsync_MultipleUsers_ReturnsAllReviews));
        var (userId1, workId) = await TestDbHelper.SeedUserAndWork(context);
        var userId2 = await TestDbHelper.SeedUser(context, "user2@test.com", "user2");
        var service = new ReviewService(context);

        await service.AddReviewAsync(userId1, this.DefaultRequest(workId, "Відгук першого юзера про твір тут"));
        await service.AddReviewAsync(userId2, this.DefaultRequest(workId, "Відгук другого юзера про твір тут"));

        var reviews = await service.GetReviewsByWorkIdAsync(workId);

        var reviewList = reviews.ToList();
        reviewList.Should().HaveCount(2);
        reviewList.Should().Contain(r => r.UserId == userId1);
        reviewList.Should().Contain(r => r.UserId == userId2);
    }

    // ── UpdateReviewAsync ─────────────────────────────────────────────────────

    /// <summary>
    /// Оновлення власного відгуку зберігає зміни.
    /// </summary>
    [Fact]
    public async Task UpdateReviewAsync_OwnReview_UpdatesSuccessfully()
    {
        using var context = TestDbHelper.CreateContext(nameof(this.UpdateReviewAsync_OwnReview_UpdatesSuccessfully));
        var (userId, workId) = await TestDbHelper.SeedUserAndWork(context);
        var service = new ReviewService(context);
        var response = await service.AddReviewAsync(userId, this.DefaultRequest(workId, "Оригінальний текст відгуку для оновлення"));

        var result = await service.UpdateReviewAsync(userId, response.ReviewId, new ReviewRequest
        {
            WorkId = workId, Text = "Оновлений текст відгуку після редагування юзером", IsSpoiler = true, Rating = 9.0, TargetType = "comparison",
        });

        result.Should().BeTrue();
        var updated = await context.Reviews.FindAsync(response.ReviewId);
        updated!.Text.Should().Be("Оновлений текст відгуку після редагування юзером");
        updated.IsSpoiler.Should().BeTrue();
        updated.Rating.Should().Be(9.0);
    }

    /// <summary>
    /// Оновлення чужого відгуку кидає ForbiddenException.
    /// </summary>
    [Fact]
    public async Task UpdateReviewAsync_OtherUserReview_ThrowsForbiddenException()
    {
        using var context = TestDbHelper.CreateContext(nameof(this.UpdateReviewAsync_OtherUserReview_ThrowsForbiddenException));
        var (userId, workId) = await TestDbHelper.SeedUserAndWork(context);
        var service = new ReviewService(context);
        var response = await service.AddReviewAsync(userId, this.DefaultRequest(workId, "Відгук власника якого хочуть змінити"));

        await service.Invoking(s => s.UpdateReviewAsync(Guid.NewGuid(), response.ReviewId, this.DefaultRequest(workId, "Спроба змінити чужий відгук хакером")))
            .Should().ThrowAsync<ForbiddenException>();
    }

    // ── DeleteReviewAsync ─────────────────────────────────────────────────────

    /// <summary>
    /// Видалення власного відгуку видаляє з БД.
    /// </summary>
    [Fact]
    public async Task DeleteReviewAsync_OwnReview_DeletesFromDB()
    {
        using var context = TestDbHelper.CreateContext(nameof(this.DeleteReviewAsync_OwnReview_DeletesFromDB));
        var (userId, workId) = await TestDbHelper.SeedUserAndWork(context);
        var service = new ReviewService(context);
        var response = await service.AddReviewAsync(userId, this.DefaultRequest(workId, "Відгук який буде видалений власником"));

        var result = await service.DeleteReviewAsync(userId, response.ReviewId);

        result.Should().BeTrue();
        var deleted = await context.Reviews.FindAsync(response.ReviewId);
        deleted.Should().BeNull();
    }

    /// <summary>
    /// Видалення чужого відгуку кидає ForbiddenException.
    /// </summary>
    [Fact]
    public async Task DeleteReviewAsync_OtherUserReview_ThrowsForbiddenException()
    {
        using var context = TestDbHelper.CreateContext(nameof(this.DeleteReviewAsync_OtherUserReview_ThrowsForbiddenException));
        var (userId, workId) = await TestDbHelper.SeedUserAndWork(context);
        var service = new ReviewService(context);
        var response = await service.AddReviewAsync(userId, this.DefaultRequest(workId, "Відгук власника якого хочуть видалити"));

        await service.Invoking(s => s.DeleteReviewAsync(Guid.NewGuid(), response.ReviewId))
            .Should().ThrowAsync<ForbiddenException>();
    }

    // ── ModerateReviewAsync ───────────────────────────────────────────────────

    /// <summary>
    /// Approve видаляє відгук і ставить статус Resolved.
    /// </summary>
    [Fact]
    public async Task ModerateReviewAsync_Approve_DeletesReviewAndResolvesReport()
    {
        using var context = TestDbHelper.CreateContext(nameof(this.ModerateReviewAsync_Approve_DeletesReviewAndResolvesReport));
        var (userId, workId) = await TestDbHelper.SeedUserAndWork(context);
        var service = new ReviewService(context);
        var reviewResponse = await service.AddReviewAsync(userId, this.DefaultRequest(workId, "Порушуючий відгук для модерації адміном"));
        await service.ReportReviewAsync(Guid.NewGuid(), reviewResponse.ReviewId, "Спам");
        var reports = await service.GetAllReportsAsync();
        var reportId = reports.First().ReportId;

        await service.ModerateReviewAsync(reportId, "delete");

        var deletedReview = await context.Reviews.FindAsync(reviewResponse.ReviewId);
        deletedReview.Should().BeNull();
        var report = await context.Reports.FindAsync(reportId);
        report!.Status.Should().Be("Resolved");
    }

    /// <summary>
    /// Reject не видаляє відгук і ставить статус Dismissed.
    /// </summary>
    [Fact]
    public async Task ModerateReviewAsync_Reject_KeepsReviewAndDismissesReport()
    {
        using var context = TestDbHelper.CreateContext(nameof(this.ModerateReviewAsync_Reject_KeepsReviewAndDismissesReport));
        var (userId, workId) = await TestDbHelper.SeedUserAndWork(context);
        var service = new ReviewService(context);
        var reviewResponse = await service.AddReviewAsync(userId, this.DefaultRequest(workId, "Звичайний відгук що не порушує правил"));
        await service.ReportReviewAsync(Guid.NewGuid(), reviewResponse.ReviewId, "test");
        var reports = await service.GetAllReportsAsync();
        var reportId = reports.First().ReportId;

        await service.ModerateReviewAsync(reportId, "dismiss");

        var review = await context.Reviews.FindAsync(reviewResponse.ReviewId);
        review.Should().NotBeNull();
        var report = await context.Reports.FindAsync(reportId);
        report!.Status.Should().Be("Dismissed");
    }

    /// <summary>
    /// Spoiler позначає відгук IsSpoiler=true і ставить статус Resolved.
    /// </summary>
    [Fact]
    public async Task ModerateReviewAsync_Spoiler_MarksAsSpoilerAndResolvesReport()
    {
        using var context = TestDbHelper.CreateContext(nameof(this.ModerateReviewAsync_Spoiler_MarksAsSpoilerAndResolvesReport));
        var (userId, workId) = await TestDbHelper.SeedUserAndWork(context);
        var service = new ReviewService(context);
        var reviewResponse = await service.AddReviewAsync(userId, new ReviewRequest
        {
            WorkId = workId, Text = "Відгук з прихованим спойлером без позначки", IsSpoiler = false, Rating = 5.0, TargetType = "comparison",
        });
        await service.ReportReviewAsync(Guid.NewGuid(), reviewResponse.ReviewId, "Спойлер");
        var reports = await service.GetAllReportsAsync();
        var reportId = reports.First().ReportId;

        await service.ModerateReviewAsync(reportId, "spoiler");

        var review = await context.Reviews.FindAsync(reviewResponse.ReviewId);
        review!.IsSpoiler.Should().BeTrue();
        var report = await context.Reports.FindAsync(reportId);
        report!.Status.Should().Be("Resolved");
    }

    /// <summary>
    /// Невалідна дія кидає ArgumentException.
    /// </summary>
    [Fact]
    public async Task ModerateReviewAsync_InvalidAction_ThrowsArgumentException()
    {
        using var context = TestDbHelper.CreateContext(nameof(this.ModerateReviewAsync_InvalidAction_ThrowsArgumentException));
        var (userId, workId) = await TestDbHelper.SeedUserAndWork(context);
        var service = new ReviewService(context);
        var reviewResponse = await service.AddReviewAsync(userId, this.DefaultRequest(workId, "Відгук для тесту невалідної дії тут"));
        await service.ReportReviewAsync(Guid.NewGuid(), reviewResponse.ReviewId, "test");
        var reports = await service.GetAllReportsAsync();
        var reportId = reports.First().ReportId;

        await service.Invoking(s => s.ModerateReviewAsync(reportId, "invalid_action"))
            .Should().ThrowAsync<ArgumentException>();
    }
}
