namespace Book2Screen.Tests.Services;

using AutoMapper;
using Book2Screen.Application.DTOs;
using Book2Screen.Application.Mappings;
using Book2Screen.Application.Services;
using Book2Screen.Domain.Entities;
using Book2Screen.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Serilog;
using Serilog.Extensions.Logging;
using Xunit;

/// <summary>
/// Unit tests for AdaptationService using InMemoryDatabase.
/// </summary>
public class AdaptationServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly AdaptationService _service;

    public AdaptationServiceTests()
    {
        // Setup InMemory Database
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        _context = new ApplicationDbContext(options);

        // Setup AutoMapper
        var serilogLogger = new LoggerConfiguration().CreateLogger();
        var loggerFactory = new SerilogLoggerFactory(serilogLogger);
        var config = new MapperConfiguration(
            mc => mc.AddProfile(new AdaptationProfile()),
            loggerFactory);
        _mapper = config.CreateMapper();

        _service = new AdaptationService(_context, _mapper);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task CreateAdaptationAsync_ValidDto_ReturnsCreatedDto()
    {
        // Arrange
        var dto = new AdaptationDto
        {
            Title = "Test Movie",
            Type = "movie",
            Description = "Description",
            ReleaseYear = 2024
        };

        // Act
        var result = await _service.CreateAdaptationAsync(dto);

        // Assert
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(dto.Title, result.Title);
        var entity = await _context.Adaptations.FindAsync(result.Id);
        Assert.NotNull(entity);
        Assert.Equal(dto.Title, entity.Title);
    }

    [Fact]
    public async Task UpdateAdaptationAsync_ExistingId_UpdatesAndReturnsDto()
    {
        // Arrange
        var adaptation = new Adaptation { Id = Guid.NewGuid(), Title = "Old Title", Type = "movie" };
        await _context.Adaptations.AddAsync(adaptation);
        await _context.SaveChangesAsync();

        var updateDto = new AdaptationDto { Title = "New Title", Type = "series" };

        // Act
        var result = await _service.UpdateAdaptationAsync(adaptation.Id, updateDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("New Title", result.Title);
        Assert.Equal("series", result.Type);
        
        var updatedEntity = await _context.Adaptations.FindAsync(adaptation.Id);
        Assert.Equal("New Title", updatedEntity!.Title);
    }

    [Fact]
    public async Task UpdateAdaptationAsync_NonExistingId_ThrowsKeyNotFound()
    {
        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.UpdateAdaptationAsync(Guid.NewGuid(), new AdaptationDto { Title = "Title", Type = "movie" }));
    }

    [Fact]
    public async Task DeleteAdaptationAsync_ExistingId_ReturnsTrueAndRemovesEntity()
    {
        // Arrange
        var adaptation = new Adaptation { Id = Guid.NewGuid(), Title = "To Delete", Type = "movie" };
        await _context.Adaptations.AddAsync(adaptation);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.DeleteAdaptationAsync(adaptation.Id);

        // Assert
        Assert.True(result);
        var entity = await _context.Adaptations.FindAsync(adaptation.Id);
        Assert.Null(entity);
    }

    [Fact]
    public async Task CreateAdaptationAsync_WithDifferences_SetsMapTitleAndImportance()
    {
        // Arrange
        var dto = new AdaptationDto
        {
            Title = "Inception",
            Type = "movie",
            Differences = new List<DifferenceDto>
            {
                new DifferenceDto { Title = "Ending", BookText = "A", FilmText = "B", ImportanceLevel = "high" }
            }
        };

        // Act
        var result = await _service.CreateAdaptationAsync(dto);

        // Assert
        var work = await _context.Works.Include(w => w.DifferenceMap).ThenInclude(dm => dm!.Differences).FirstOrDefaultAsync(w => w.Title == "Inception");
        Assert.NotNull(work!.DifferenceMap);
        Assert.Equal("Карта розбіжностей: Inception", work.DifferenceMap.Title);
        Assert.Equal("high", work.DifferenceMap.Differences.First().ImportanceLevel);
    }
}
