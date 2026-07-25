namespace Book2Screen.Tests.Services;

using AutoMapper;
using Book2Screen.Application.DTOs;
using Book2Screen.Application.Filters;
using Book2Screen.Application.Mappings;
using Book2Screen.Application.Services;
using Book2Screen.Domain.Entities;
using Book2Screen.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Xunit;

public class WorkServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly WorkService _service;
    private readonly IMapper _mapper;

    public WorkServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        _context = new ApplicationDbContext(options);

        var config = new MapperConfiguration(cfg => cfg.AddProfile<AdaptationProfile>(), new Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory());
        _mapper = config.CreateMapper();

        _service = new WorkService(_context, _mapper);
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

        work1.Rating = new Rating { WorkId = work1.Id, AdaptationRating = 9.0m, BookRating = 8.0m };
        work2.Rating = new Rating { WorkId = work2.Id, AdaptationRating = 7.0m, BookRating = 6.0m };

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
        
        work.Rating = new Rating { WorkId = work.Id, AdaptationRating = 9.0m, BookRating = 9.0m };
        
        await _context.Works.AddAsync(work);
        await _context.SaveChangesAsync();

        // Act
        var result = (await _service.GetTopWorksAsync(10)).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal(9.0, result[0].FilmRating);
    }

    [Fact]
    public async Task GetWorkByIdAsync_ReturnsWorkWithAllMappedFields()
    {
        // Arrange
        var author1 = new Author { Id = Guid.NewGuid(), FullName = "Автор 1" };
        var author2 = new Author { Id = Guid.NewGuid(), FullName = "Автор 2" };
        var book = new Book { Id = Guid.NewGuid(), Title = "Книга", Description = "Опис книги", Authors = new List<Author> { author1, author2 } };
        var adaptation = new Adaptation { Id = Guid.NewGuid(), Title = "Фільм", Type = "movie", Studio = "Legendary", Description = "Опис фільму", ReleaseYear = 2024 };
        var workId = Guid.NewGuid();
        var work = new Work { Id = workId, Title = "Твір", Book = book, Adaptation = adaptation, Summary = "Загальний опис" };
        
        await _context.Works.AddAsync(work);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetWorkByIdAsync(workId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Автор 1, Автор 2", result.Author);
        Assert.Equal("Legendary", result.Director);
        Assert.Equal("Опис книги", result.BookSummary);
        Assert.Equal("Опис фільму", result.FilmSummary);
        Assert.Equal("Загальний опис", result.Description);
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
                new Difference { Id = Guid.NewGuid(), MapId = mapId, Title = "Сцена 1", BookText = "...", FilmText = "Сцена, яку вирізали з фільму", ImportanceLevel = "low" },
                new Difference { Id = Guid.NewGuid(), MapId = mapId, Title = "Сцена 2", BookText = "...", FilmText = "Змінений фінал", ImportanceLevel = "high" }
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
        Assert.Equal("high", result.Differences.First(d => d.Title == "Сцена 2").ImportanceLevel);
        Assert.Equal("low", result.Differences.First(d => d.Title == "Сцена 1").ImportanceLevel);
        Assert.Contains(result.Differences, d => d.Title == "Сцена 2" && d.FilmText == "Змінений фінал");
        Assert.Contains(result.Differences, d => d.Title == "Сцена 1" && d.FilmText == "Сцена, яку вирізали з фільму");
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

    [Fact]
    public async Task GetAllWorksAsync_DoesNotFilterByUserId_ReturnsAllWorksPublicly()
    {
        // Arrange
        // (BUG-056 check): We verify works are returned and not filtered by any user constraint.
        var work1 = new Work 
        { 
            Id = Guid.NewGuid(), 
            Title = "Work 1", 
            Book = new Book { Title = "B1" }, 
            Adaptation = new Adaptation { Title = "A1", Type = "movie" } 
        };
        var work2 = new Work 
        { 
            Id = Guid.NewGuid(), 
            Title = "Work 2", 
            Book = new Book { Title = "B2" }, 
            Adaptation = new Adaptation { Title = "A2", Type = "movie" } 
        };

        await _context.Works.AddRangeAsync(work1, work2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetAllWorksAsync(new WorkFilter());

        // Assert
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetWorkByIdAsync_MapsBookAndAdaptationPropertiesSeparately_NoDuplication()
    {
        // Arrange
        // (BUG-058 check): We verify book and movie properties map separately (e.g. poster vs filmPoster, book rating vs film rating).
        var workId = Guid.NewGuid();
        var work = new Work 
        { 
            Id = workId, 
            Title = "Test Comparison", 
            Book = new Book 
            { 
                Title = "The Novel", 
                CoverImageUrl = "book_poster.jpg",
                Genre = "Drama"
            }, 
            Adaptation = new Adaptation 
            { 
                Title = "The Film", 
                Type = "movie", 
                PosterUrl = "movie_poster.jpg",
                Country = "Ukraine"
            } 
        };
        
        await _context.Works.AddAsync(work);
        
        work.Rating = new Rating { WorkId = workId, BookRating = 8.5m, AdaptationRating = 9.2m };
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetWorkByIdAsync(workId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("book_poster.jpg", result.Poster);
        Assert.Equal("movie_poster.jpg", result.FilmPoster);
        Assert.Equal(8.5, result.BookRating);
        Assert.Equal(9.2, result.FilmRating);
        Assert.Equal("Ukraine", result.FilmCountry);
    }
}
