namespace Book2Screen.Tests.Services;

using Book2Screen.Application.DTOs;
using Book2Screen.Application.Filters;
using Book2Screen.Application.Services;
using Book2Screen.Domain.Entities;
using Book2Screen.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Xunit;

public class WorkServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly VoteService _voteService;
    private readonly WorkService _service;

    public WorkServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
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
    public async Task GetWorkByIdAsync_ThrowsKeyNotFound_WhenWorkDoesNotExist()
    {
        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.GetWorkByIdAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetTopWorksAsync_ReturnsSortedWorksByRating()
    {
        // Arrange
        var book1 = new Book { Id = Guid.NewGuid(), Title = "Книга 1" };
        var adaptation1 = new Adaptation { Id = Guid.NewGuid(), Title = "Фільм 1", Type = "movie" };
        var work1 = new Work { Id = Guid.NewGuid(), Title = "Твір 1", Book = book1, Adaptation = adaptation1 };

        var book2 = new Book { Id = Guid.NewGuid(), Title = "Книга 2" };
        var adaptation2 = new Adaptation { Id = Guid.NewGuid(), Title = "Фільм 2", Type = "movie" };
        var work2 = new Work { Id = Guid.NewGuid(), Title = "Твір 2", Book = book2, Adaptation = adaptation2 };

        await _context.Works.AddRangeAsync(work1, work2);

        var review1 = new Review { WorkId = work1.Id, Rating = 9.0, TargetType = "adaptation", Text = "Чудово" };
        var review2 = new Review { WorkId = work2.Id, Rating = 7.0, TargetType = "adaptation", Text = "Нормально" };

        await _context.Reviews.AddRangeAsync(review1, review2);
        await _context.SaveChangesAsync();

        // Act
        var result = (await _service.GetTopWorksAsync(10)).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("Твір 1", result[0].Title);
        Assert.Equal(9.0, result[0].FilmRating);
        Assert.Equal("Твір 2", result[1].Title);
        Assert.Equal(7.0, result[1].FilmRating);
    }

    [Fact]
    public async Task GetTopWorksAsync_AveragesMultipleReviews()
    {
        // Arrange
        var book = new Book { Id = Guid.NewGuid(), Title = "Книга" };
        var adaptation = new Adaptation { Id = Guid.NewGuid(), Title = "Фільм", Type = "movie" };
        var work = new Work { Id = Guid.NewGuid(), Title = "Твір", Book = book, Adaptation = adaptation };
        await _context.Works.AddAsync(work);

        var review1 = new Review { WorkId = work.Id, Rating = 10.0, TargetType = "adaptation", Text = "Супер" };
        var review2 = new Review { WorkId = work.Id, Rating = 8.0, TargetType = "adaptation", Text = "Добре" };
        await _context.Reviews.AddRangeAsync(review1, review2);
        await _context.SaveChangesAsync();

        // Act
        var result = (await _service.GetTopWorksAsync(10)).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal(9.0, result[0].FilmRating);
    }

    [Fact]
    public async Task GetWorkByIdAsync_ReturnsWorkWithDifferences()
    {
        // Arrange
        var book = new Book { Id = Guid.NewGuid(), Title = "Книга" };
        var adaptation = new Adaptation { Id = Guid.NewGuid(), Title = "Фільм", Type = "movie" };
        var workId = Guid.NewGuid();
        var work = new Work { Id = workId, Title = "Твір", Book = book, Adaptation = adaptation };
        
        var mapId = Guid.NewGuid();
        var map = new DifferenceMap 
        { 
            Id = mapId, 
            WorkId = workId, 
            Title = "Карта",
            Differences = new List<Difference>
            {
                new Difference { Id = Guid.NewGuid(), MapId = mapId, DifferenceType = "Вирізана сцена", Description = "Сцена, яку вирізали з фільму", ImportanceLevel = "low" },
                new Difference { Id = Guid.NewGuid(), MapId = mapId, DifferenceType = "Зміна сюжету", Description = "Змінений фінал", ImportanceLevel = "high" }
            }
        };
        work.DifferenceMap = map;

        await _context.Works.AddAsync(work);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetWorkByIdAsync(workId);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.HasMap);
        Assert.Equal(2, result.Differences.Count);
        Assert.Contains(result.Differences, d => d.Title == "Зміна сюжету" && d.IsSpoiler);
        Assert.Contains(result.Differences, d => d.Title == "Вирізана сцена" && !d.IsSpoiler);
    }

    [Fact]
    public async Task GetAllWorksAsync_FiltersByOnlyWithMap()
    {
        // Arrange
        var work1 = new Work 
        { 
            Id = Guid.NewGuid(), 
            Title = "З картою", 
            Book = new Book { Title = "Б1" }, 
            Adaptation = new Adaptation { Title = "А1", Type = "movie" } 
        };
        work1.DifferenceMap = new DifferenceMap { Id = Guid.NewGuid(), Title = "Карта" };
        
        var work2 = new Work 
        { 
            Id = Guid.NewGuid(), 
            Title = "Без карти", 
            Book = new Book { Title = "Б2" }, 
            Adaptation = new Adaptation { Title = "А2", Type = "movie" } 
        };

        await _context.Works.AddRangeAsync(work1, work2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetAllWorksAsync(new WorkFilter { OnlyWithMap = true });

        // Assert
        Assert.Single(result);
        Assert.Equal("З картою", result.First().Title);
    }
}
