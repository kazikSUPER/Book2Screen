// Book2Screen.Tests/Services/FavoriteServiceTests.cs
namespace Book2Screen.Tests.Services;

using AutoMapper;
using Book2Screen.Application.Interfaces;
using Book2Screen.Application.Mappings;
using Book2Screen.Application.Services;
using Book2Screen.Domain.Entities;
using Book2Screen.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Extensions.Logging;
using Xunit;

public class FavoriteServiceTests
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IFavoriteService _service;

    public FavoriteServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);

        var serilogLogger = new LoggerConfiguration().CreateLogger();
        var loggerFactory = new SerilogLoggerFactory(serilogLogger);
        var config = new MapperConfiguration(
            mc => mc.AddProfile(new AdaptationProfile()),
            loggerFactory);
        _mapper = config.CreateMapper();

        _service = new FavoriteService(_context, _mapper);
    }

    [Fact]
    public async Task AddToFavoritesAsync_ShouldAddFavorite_WhenNotExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var workId = Guid.NewGuid();

        // Act
        var result = await _service.AddToFavoritesAsync(userId, workId);

        // Assert
        Assert.True(result);
        var favorite = await _context.Favorites.FirstOrDefaultAsync(f => f.UserId == userId && f.WorkId == workId);
        Assert.NotNull(favorite);
    }

    [Fact]
    public async Task AddToFavoritesAsync_ShouldReturnTrue_WhenAlreadyExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var workId = Guid.NewGuid();
        await _service.AddToFavoritesAsync(userId, workId);

        // Act
        var result = await _service.AddToFavoritesAsync(userId, workId);

        // Assert
        Assert.True(result);
        var count = await _context.Favorites.CountAsync(f => f.UserId == userId && f.WorkId == workId);
        Assert.Equal(1, count);
    }

    [Fact]
    public async Task RemoveFromFavoritesAsync_ShouldRemoveFavorite_WhenExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var workId = Guid.NewGuid();
        await _service.AddToFavoritesAsync(userId, workId);

        // Act
        var result = await _service.RemoveFromFavoritesAsync(userId, workId);

        // Assert
        Assert.True(result);
        var favorite = await _context.Favorites.FirstOrDefaultAsync(f => f.UserId == userId && f.WorkId == workId);
        Assert.Null(favorite);
    }

    [Fact]
    public async Task RemoveFromFavoritesAsync_ShouldReturnTrue_WhenNotExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var workId = Guid.NewGuid();

        // Act
        var result = await _service.RemoveFromFavoritesAsync(userId, workId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task IsFavoriteAsync_ShouldReturnTrue_WhenExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var workId = Guid.NewGuid();
        await _service.AddToFavoritesAsync(userId, workId);

        // Act
        var result = await _service.IsFavoriteAsync(userId, workId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task IsFavoriteAsync_ShouldReturnFalse_WhenNotExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var workId = Guid.NewGuid();

        // Act
        var result = await _service.IsFavoriteAsync(userId, workId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetUserFavoritesAsync_ShouldReturnMappedFavorites()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var book = new Book { Title = "Book Title" };
        var adaptation = new Adaptation { Title = "Movie Title", Type = "movie" };
        var work = new Work { Title = "Work Title", Book = book, Adaptation = adaptation };
        
        await _context.Works.AddAsync(work);
        await _context.SaveChangesAsync();

        await _service.AddToFavoritesAsync(userId, work.Id);

        // Act
        var favorites = await _service.GetUserFavoritesAsync(userId);

        // Assert
        Assert.Single(favorites);
        Assert.Equal("Work Title", favorites.First().Title);
    }
}
