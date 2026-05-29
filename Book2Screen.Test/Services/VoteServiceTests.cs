namespace Book2Screen.Tests.Services;

using Book2Screen.Application.DTOs;
using Book2Screen.Application.Services;
using Book2Screen.Domain.Entities;
using Book2Screen.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Xunit;

public class VoteServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly VoteService _service;

    public VoteServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        _context = new ApplicationDbContext(options);
        _service = new VoteService(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task VoteAsync_CreatesNewVote()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var workId = Guid.NewGuid();
        var request = new VoteRequest { WorkId = workId, VoteType = "BOOK" };

        // Act
        var result = await _service.VoteAsync(userId, request);

        // Assert
        Assert.Equal(1, result.TotalVotes);
        Assert.Equal(1, result.BookVotes);
        Assert.Equal(0, result.MovieVotes);
        Assert.Equal(100, result.BookPercentage);
        
        var voteInDb = await _context.Votes.FirstOrDefaultAsync();
        Assert.NotNull(voteInDb);
        Assert.Equal("book", voteInDb.SelectedOption);
    }

    [Fact]
    public async Task VoteAsync_UpdatesExistingVote()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var workId = Guid.NewGuid();
        await _service.VoteAsync(userId, new VoteRequest { WorkId = workId, VoteType = "BOOK" });

        // Act
        var result = await _service.VoteAsync(userId, new VoteRequest { WorkId = workId, VoteType = "MOVIE" });

        // Assert
        Assert.Equal(1, result.TotalVotes);
        Assert.Equal(0, result.BookVotes);
        Assert.Equal(1, result.MovieVotes);
        Assert.Equal(100, result.MoviePercentage);
        
        var votesCount = await _context.Votes.CountAsync();
        Assert.Equal(1, votesCount);
    }

    [Fact]
    public async Task GetVoteStatsAsync_ReturnsCorrectPercentages()
    {
        // Arrange
        var workId = Guid.NewGuid();
        await _service.VoteAsync(Guid.NewGuid(), new VoteRequest { WorkId = workId, VoteType = "BOOK" });
        await _service.VoteAsync(Guid.NewGuid(), new VoteRequest { WorkId = workId, VoteType = "BOOK" });
        await _service.VoteAsync(Guid.NewGuid(), new VoteRequest { WorkId = workId, VoteType = "MOVIE" });

        // Act
        var result = await _service.GetVoteStatsAsync(workId);

        // Assert
        Assert.Equal(3, result.TotalVotes);
        Assert.Equal(2, result.BookVotes);
        Assert.Equal(1, result.MovieVotes);
        Assert.True(Math.Abs(66.67 - result.BookPercentage) < 0.1);
        Assert.True(Math.Abs(33.33 - result.MoviePercentage) < 0.1);
    }
}
