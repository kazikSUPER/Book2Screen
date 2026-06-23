// <copyright file="VoteServiceTests.cs" company="Team 17">
// Copyright (c) Team 17. All rights reserved.
// </copyright>

namespace Book2Screen.Tests.Services;

using Book2Screen.Application.DTOs;
using Book2Screen.Application.Services;
using Book2Screen.Domain.Entities;
using Book2Screen.Tests.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

/// <summary>
/// Юніт тести для VoteService.
/// </summary>
public class VoteServiceTests
{
    // ── VoteAsync ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Голосування за книгу зберігається і повертає правильну статистику.
    /// </summary>
    [Fact]
    public async Task VoteAsync_VoteForBook_SavesAndReturnsStats()
    {
        using var context = TestDbHelper.CreateContext(nameof(this.VoteAsync_VoteForBook_SavesAndReturnsStats));
        var (userId, workId) = await TestDbHelper.SeedUserAndWork(context);
        var service = new VoteService(context);

        var result = await service.VoteAsync(userId, new VoteRequest { WorkId = workId, VoteType = "book" });

        result.TotalVotes.Should().Be(1);
        result.BookVotes.Should().Be(1);
        result.MovieVotes.Should().Be(0);
        result.BookPercentage.Should().Be(100.0);
        result.MoviePercentage.Should().Be(0.0);

        var vote = await context.Votes.FirstAsync(v => v.UserId == userId);
        vote.SelectedOption.Should().Be("book");
    }

    /// <summary>
    /// Голосування за фільм зберігається як "movie".
    /// </summary>
    [Fact]
    public async Task VoteAsync_VoteForMovie_SavesAsMovie()
    {
        using var context = TestDbHelper.CreateContext(nameof(this.VoteAsync_VoteForMovie_SavesAsMovie));
        var (userId, workId) = await TestDbHelper.SeedUserAndWork(context);
        var service = new VoteService(context);

        var result = await service.VoteAsync(userId, new VoteRequest { WorkId = workId, VoteType = "movie" });

        result.MovieVotes.Should().Be(1);
        result.BookVotes.Should().Be(0);
        var vote = await context.Votes.FirstAsync(v => v.UserId == userId);
        vote.SelectedOption.Should().Be("movie");
    }

    /// <summary>
    /// VoteType конвертується до нижнього регістру при збереженні.
    /// (BUG-034 — перевірка що бекенд нормалізує регістр)
    /// </summary>
    [Theory]
    [InlineData("BOOK", "book")]
    [InlineData("Book", "book")]
    [InlineData("MOVIE", "movie")]
    [InlineData("Movie", "movie")]
    public async Task VoteAsync_VoteTypeNormalizedToLowerCase(string input, string expected)
    {
        using var context = TestDbHelper.CreateContext($"VoteType_{input}");
        var (userId, workId) = await TestDbHelper.SeedUserAndWork(context);
        var service = new VoteService(context);

        await service.VoteAsync(userId, new VoteRequest { WorkId = workId, VoteType = input });

        var vote = await context.Votes.FirstAsync(v => v.UserId == userId);
        vote.SelectedOption.Should().Be(expected);
    }

    /// <summary>
    /// Повторне голосування оновлює вибір без дубліката.
    /// </summary>
    [Fact]
    public async Task VoteAsync_SecondVote_UpdatesExistingWithoutDuplicate()
    {
        using var context = TestDbHelper.CreateContext(nameof(this.VoteAsync_SecondVote_UpdatesExistingWithoutDuplicate));
        var (userId, workId) = await TestDbHelper.SeedUserAndWork(context);
        var service = new VoteService(context);
        await service.VoteAsync(userId, new VoteRequest { WorkId = workId, VoteType = "book" });

        var result = await service.VoteAsync(userId, new VoteRequest { WorkId = workId, VoteType = "movie" });

        result.TotalVotes.Should().Be(1);
        result.BookVotes.Should().Be(0);
        result.MovieVotes.Should().Be(1);

        var count = await context.Votes.CountAsync(v => v.UserId == userId && v.WorkId == workId);
        count.Should().Be(1);
    }

    // ── GetVoteStatsAsync ─────────────────────────────────────────────────────

    /// <summary>
    /// Статистика для твору без голосів повертає 0%.
    /// </summary>
    [Fact]
    public async Task GetVoteStatsAsync_NoVotes_ReturnsZeroPercentages()
    {
        using var context = TestDbHelper.CreateContext(nameof(this.GetVoteStatsAsync_NoVotes_ReturnsZeroPercentages));
        var (_, workId) = await TestDbHelper.SeedUserAndWork(context);
        var service = new VoteService(context);

        var result = await service.GetVoteStatsAsync(workId);

        result.TotalVotes.Should().Be(0);
        result.BookPercentage.Should().Be(0.0);
        result.MoviePercentage.Should().Be(0.0);
    }

    /// <summary>
    /// Статистика коректно рахує відсотки (75% книга, 25% фільм).
    /// </summary>
    [Fact]
    public async Task GetVoteStatsAsync_MultipleVotes_CalculatesCorrectPercentages()
    {
        using var context = TestDbHelper.CreateContext(nameof(this.GetVoteStatsAsync_MultipleVotes_CalculatesCorrectPercentages));
        var (userId1, workId) = await TestDbHelper.SeedUserAndWork(context);
        var userId2 = await TestDbHelper.SeedUser(context, "u2@test.com", "u2");
        var userId3 = await TestDbHelper.SeedUser(context, "u3@test.com", "u3");
        var userId4 = await TestDbHelper.SeedUser(context, "u4@test.com", "u4");
        var service = new VoteService(context);

        await service.VoteAsync(userId1, new VoteRequest { WorkId = workId, VoteType = "book" });
        await service.VoteAsync(userId2, new VoteRequest { WorkId = workId, VoteType = "book" });
        await service.VoteAsync(userId3, new VoteRequest { WorkId = workId, VoteType = "book" });
        await service.VoteAsync(userId4, new VoteRequest { WorkId = workId, VoteType = "movie" });

        var result = await service.GetVoteStatsAsync(workId);

        result.TotalVotes.Should().Be(4);
        result.BookVotes.Should().Be(3);
        result.MovieVotes.Should().Be(1);
        result.BookPercentage.Should().BeApproximately(75.0, 0.01);
        result.MoviePercentage.Should().BeApproximately(25.0, 0.01);
    }
}
