namespace Book2Screen.Tests.Services;

using Book2Screen.Application.DTOs;
using Book2Screen.Application.Services;
using Book2Screen.Domain.Entities;
using Book2Screen.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

public class WorkServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly WorkService _service;

    public WorkServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);
        _service = new WorkService(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task GetTopWorksAsync_ReturnsSortedWorksByRating()
    {
        // Arrange
        var book1 = new Book { Id = Guid.NewGuid(), Title = "Book 1" };
        var adaptation1 = new Adaptation { Id = Guid.NewGuid(), Title = "Movie 1", Type = "movie" };
        var work1 = new Work { Id = Guid.NewGuid(), Title = "Work 1", Book = book1, Adaptation = adaptation1 };

        var book2 = new Book { Id = Guid.NewGuid(), Title = "Book 2" };
        var adaptation2 = new Adaptation { Id = Guid.NewGuid(), Title = "Movie 2", Type = "movie" };
        var work2 = new Work { Id = Guid.NewGuid(), Title = "Work 2", Book = book2, Adaptation = adaptation2 };

        await _context.Works.AddRangeAsync(work1, work2);

        var review1 = new Review { WorkId = work1.Id, Rating = 9.0, TargetType = "adaptation", Text = "Good" };
        var review2 = new Review { WorkId = work2.Id, Rating = 7.0, TargetType = "adaptation", Text = "Okay" };

        await _context.Reviews.AddRangeAsync(review1, review2);
        await _context.SaveChangesAsync();

        // Act
        var result = (await _service.GetTopWorksAsync(10)).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("Work 1", result[0].Title);
        Assert.Equal(9.0, result[0].FilmRating);
        Assert.Equal("Work 2", result[1].Title);
        Assert.Equal(7.0, result[1].FilmRating);
    }

    [Fact]
    public async Task GetTopWorksAsync_AveragesMultipleReviews()
    {
        // Arrange
        var book = new Book { Id = Guid.NewGuid(), Title = "Book" };
        var adaptation = new Adaptation { Id = Guid.NewGuid(), Title = "Movie", Type = "movie" };
        var work = new Work { Id = Guid.NewGuid(), Title = "Work", Book = book, Adaptation = adaptation };
        await _context.Works.AddAsync(work);

        var review1 = new Review { WorkId = work.Id, Rating = 10.0, TargetType = "adaptation", Text = "Great" };
        var review2 = new Review { WorkId = work.Id, Rating = 8.0, TargetType = "adaptation", Text = "Good" };
        await _context.Reviews.AddRangeAsync(review1, review2);
        await _context.SaveChangesAsync();

        // Act
        var result = (await _service.GetTopWorksAsync(10)).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal(9.0, result[0].FilmRating);
    }
}
