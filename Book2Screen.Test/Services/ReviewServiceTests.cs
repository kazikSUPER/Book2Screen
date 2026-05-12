namespace Book2Screen.Tests.Services;

using Book2Screen.Application.DTOs;
using Book2Screen.Application.Services;
using Book2Screen.Domain.Entities;
using Book2Screen.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

public class ReviewServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly ReviewService _service;

    public ReviewServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);
        _service = new ReviewService(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task AddReviewAsync_CreatesReview_WhenWorkExists()
    {
        // Arrange
        var workId = Guid.NewGuid();
        var work = new Work { Id = workId, Title = "Test Work" };
        await _context.Works.AddAsync(work);
        await _context.SaveChangesAsync();

        var userId = Guid.NewGuid();
        var request = new ReviewRequest 
        { 
            WorkId = workId, 
            Text = "This is a great adaptation!", 
            IsSpoiler = false, 
            Rating = 9.5,
            TargetType = "adaptation"
        };

        // Act
        var result = await _service.AddReviewAsync(userId, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.Text, result.Text);
        Assert.Equal(request.Rating, result.Rating);
        Assert.Equal("adaptation", result.TargetType);
        
        var reviewInDb = await _context.Reviews.FirstOrDefaultAsync(r => r.Id == result.ReviewId);
        Assert.NotNull(reviewInDb);
        Assert.Equal(userId, reviewInDb.UserId);
    }

    [Fact]
    public async Task AddReviewAsync_ThrowsKeyNotFoundException_WhenWorkDoesNotExist()
    {
        // Arrange
        var request = new ReviewRequest 
        { 
            WorkId = Guid.NewGuid(), 
            Text = "Review for non-existent work", 
            IsSpoiler = false, 
            Rating = 5.0 
        };

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.AddReviewAsync(Guid.NewGuid(), request));
    }

    [Fact]
    public async Task GetReviewsByWorkIdAsync_ReturnsReviewsInDescendingOrder()
    {
        // Arrange
        var workId = Guid.NewGuid();
        var work = new Work { Id = workId, Title = "Ordered Reviews Work" };
        await _context.Works.AddAsync(work);

        var r1 = new Review { WorkId = workId, Text = "Old Review", CreatedAt = DateTime.UtcNow.AddHours(-1), TargetType = "book" };
        var r2 = new Review { WorkId = workId, Text = "New Review", CreatedAt = DateTime.UtcNow, TargetType = "book" };
        
        await _context.Reviews.AddRangeAsync(r1, r2);
        await _context.SaveChangesAsync();

        // Act
        var result = (await _service.GetReviewsByWorkIdAsync(workId)).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("New Review", result[0].Text);
        Assert.Equal("Old Review", result[1].Text);
    }
}
