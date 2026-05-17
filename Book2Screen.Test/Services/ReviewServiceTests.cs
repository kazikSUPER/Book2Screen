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

    [Fact]
    public async Task UpdateReviewAsync_UpdatesReview_WhenUserIsOwner()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var reviewId = Guid.NewGuid();
        var workId = Guid.NewGuid();
        var review = new Review 
        { 
            Id = reviewId, 
            UserId = userId, 
            WorkId = workId,
            Text = "Original Text", 
            Rating = 5.0, 
            TargetType = "book" 
        };
        await _context.Reviews.AddAsync(review);
        await _context.SaveChangesAsync();

        var request = new ReviewRequest 
        { 
            WorkId = workId,
            Text = "Updated Text", 
            Rating = 8.0, 
            IsSpoiler = true, 
            TargetType = "adaptation" 
        };

        // Act
        var result = await _service.UpdateReviewAsync(userId, reviewId, request);

        // Assert
        Assert.True(result);
        var updatedReview = await _context.Reviews.FindAsync(reviewId);
        Assert.Equal("Updated Text", updatedReview!.Text);
        Assert.Equal(8.0, updatedReview.Rating);
        Assert.True(updatedReview.IsSpoiler);
        Assert.Equal("adaptation", updatedReview.TargetType);
    }

    [Fact]
    public async Task UpdateReviewAsync_ReturnsFalse_WhenUserIsNotOwner()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var reviewId = Guid.NewGuid();
        var workId = Guid.NewGuid();
        var review = new Review { Id = reviewId, UserId = otherUserId, WorkId = workId, Text = "Original Text", TargetType = "book" };
        await _context.Reviews.AddAsync(review);
        await _context.SaveChangesAsync();

        var request = new ReviewRequest 
        { 
            WorkId = workId, 
            Text = "Updated Text", 
            IsSpoiler = false, 
            Rating = 5.0,
            TargetType = "book" 
        };

        // Act
        var result = await _service.UpdateReviewAsync(userId, reviewId, request);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task DeleteReviewAsync_DeletesReview_WhenUserIsOwner()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var reviewId = Guid.NewGuid();
        var review = new Review { Id = reviewId, UserId = userId, WorkId = Guid.NewGuid(), Text = "To delete", TargetType = "book" };
        await _context.Reviews.AddAsync(review);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.DeleteReviewAsync(userId, reviewId);

        // Assert
        Assert.True(result);
        Assert.Null(await _context.Reviews.FindAsync(reviewId));
    }

    [Fact]
    public async Task GetUserReviewsAsync_ReturnsOnlyUserReviews()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        
        var r1 = new Review { UserId = userId, WorkId = Guid.NewGuid(), Text = "My Review", TargetType = "book" };
        var r2 = new Review { UserId = otherUserId, WorkId = Guid.NewGuid(), Text = "Other Review", TargetType = "book" };
        
        await _context.Reviews.AddRangeAsync(r1, r2);
        await _context.SaveChangesAsync();

        // Act
        var result = (await _service.GetUserReviewsAsync(userId)).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal("My Review", result[0].Text);
    }

    [Fact]
    public async Task ReportReviewAsync_CreatesReport()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var reviewId = Guid.NewGuid();
        var reason = "Spam";

        // Act
        await _service.ReportReviewAsync(userId, reviewId, reason);

        // Assert
        var report = await _context.Reports.FirstOrDefaultAsync(r => r.ReviewId == reviewId);
        Assert.NotNull(report);
        Assert.Equal(reason, report.Reason);
        Assert.Equal("Pending", report.Status);
    }

    [Fact]
    public async Task ModerateReviewAsync_Approve_RemovesReview()
    {
        // Arrange
        var reviewId = Guid.NewGuid();
        var review = new Review { Id = reviewId, Text = "Bad text", TargetType = "book" };
        var reportId = Guid.NewGuid();
        var report = new Report { Id = reportId, ReviewId = reviewId, Status = "Pending", Reason = "X" };
        
        await _context.Reviews.AddAsync(review);
        await _context.Reports.AddAsync(report);
        await _context.SaveChangesAsync();

        // Act
        await _service.ModerateReviewAsync(reportId, "approve");

        // Assert
        Assert.Null(await _context.Reviews.FindAsync(reviewId));
        var updatedReport = await _context.Reports.FindAsync(reportId);
        Assert.Equal("Resolved", updatedReport!.Status);
    }

    [Fact]
    public async Task ModerateReviewAsync_Spoiler_SetsIsSpoiler()
    {
        // Arrange
        var reviewId = Guid.NewGuid();
        var review = new Review { Id = reviewId, Text = "Plot twist inside", IsSpoiler = false, TargetType = "book" };
        var reportId = Guid.NewGuid();
        var report = new Report { Id = reportId, ReviewId = reviewId, Status = "Pending", Reason = "Spoiler" };
        
        await _context.Reviews.AddAsync(review);
        await _context.Reports.AddAsync(report);
        await _context.SaveChangesAsync();

        // Act
        await _service.ModerateReviewAsync(reportId, "spoiler");

        // Assert
        var updatedReview = await _context.Reviews.FindAsync(reviewId);
        Assert.True(updatedReview!.IsSpoiler);
        var updatedReport = await _context.Reports.FindAsync(reportId);
        Assert.Equal("Resolved", updatedReport!.Status);
    }
}
