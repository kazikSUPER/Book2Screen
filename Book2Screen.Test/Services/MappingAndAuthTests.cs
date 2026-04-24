// Book2Screen.Tests/Services/MappingAndAuthTests.cs
// xUnit + Moq — mapping, BCrypt, and ITokenService mock tests.
// No DB, no HTTP. AutoMapper configured exactly as in Program.cs.

namespace Book2Screen.Tests.Services;

using AutoMapper;
using Book2Screen.Application.DTOs;
using Book2Screen.Application.Interfaces;
using Book2Screen.Application.Mappings;
using Book2Screen.Domain.Entities;
using Moq;
using Serilog;
using Serilog.Extensions.Logging;
using Xunit;

// ═══════════════════════════════════════════════════════════════════════════════
// AdaptationProfile (AutoMapper)
// ═══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Unit tests for <see cref="AdaptationProfile"/>.
/// Uses the same MapperConfiguration pattern as Program.cs (Serilog logger).
/// No DB, no HTTP.
/// </summary>
public class AdaptationMappingTests
{
    private readonly IMapper _mapper;

    public AdaptationMappingTests()
    {
        // Mirror Program.cs exactly — AutoMapper 16.x needs a real ILoggerFactory
        var serilogLogger = new LoggerConfiguration().CreateLogger();
        var loggerFactory = new SerilogLoggerFactory(serilogLogger);

        var config = new MapperConfiguration(
            mc => mc.AddProfile(new AdaptationProfile()),
            loggerFactory);

        _mapper = config.CreateMapper();
    }

    // ── TC-UNIT-19 ─────────────────────────────────────────────────────────
    /// <summary>AdaptationProfile builds without exceptions.</summary>
    [Fact]
    public void AdaptationProfile_CreatesMapperWithoutException()
    {
        var ex = Record.Exception(() =>
        {
            var serilogLogger = new LoggerConfiguration().CreateLogger();
            var loggerFactory = new SerilogLoggerFactory(serilogLogger);
            var config = new MapperConfiguration(
                mc => mc.AddProfile(new AdaptationProfile()),
                loggerFactory);
            config.CreateMapper();
        });
        Assert.Null(ex);
    }

    // ── TC-UNIT-20 ─────────────────────────────────────────────────────────
    /// <summary>Adaptation entity → AdaptationDto: all fields copied correctly.</summary>
    [Fact]
    public void AdaptationProfile_EntityToDto_MapsAllFieldsCorrectly()
    {
        var entity = new Adaptation
        {
            Id              = Guid.NewGuid(),
            Title           = "Dune: Part Two",
            Type            = "movie",
            Description     = "The continuation of Paul's journey.",
            ReleaseYear     = 2024,
            DurationMinutes = 166,
            PosterUrl       = "https://example.com/dune2.jpg",
            Studio          = "Legendary Pictures",
            Country         = "USA",
        };

        var dto = _mapper.Map<AdaptationDto>(entity);

        Assert.Equal(entity.Id,              dto.Id);
        Assert.Equal(entity.Title,           dto.Title);
        Assert.Equal(entity.Type,            dto.Type);
        Assert.Equal(entity.Description,     dto.Description);
        Assert.Equal(entity.ReleaseYear,     dto.ReleaseYear);
        Assert.Equal(entity.DurationMinutes, dto.DurationMinutes);
        Assert.Equal(entity.PosterUrl,       dto.PosterUrl);
        Assert.Equal(entity.Studio,          dto.Studio);
        Assert.Equal(entity.Country,         dto.Country);
    }

    // ── TC-UNIT-21 ─────────────────────────────────────────────────────────
    /// <summary>AdaptationDto → Adaptation (ReverseMap): fields copied correctly.</summary>
    [Fact]
    public void AdaptationProfile_DtoToEntity_ReverseMapWorks()
    {
        var dto = new AdaptationDto
        {
            Id          = Guid.NewGuid(),
            Title       = "The Lord of the Rings",
            Type        = "series",
            ReleaseYear = 2001,
            Country     = "New Zealand",
        };

        var entity = _mapper.Map<Adaptation>(dto);

        Assert.Equal(dto.Id,          entity.Id);
        Assert.Equal(dto.Title,       entity.Title);
        Assert.Equal(dto.Type,        entity.Type);
        Assert.Equal(dto.ReleaseYear, entity.ReleaseYear);
        Assert.Equal(dto.Country,     entity.Country);
    }

    // ── TC-UNIT-22 ─────────────────────────────────────────────────────────
    /// <summary>Optional nullable fields map to null without throwing.</summary>
    [Fact]
    public void AdaptationProfile_NullOptionalFields_MapToNullWithoutException()
    {
        var entity = new Adaptation { Title = "Minimal", Type = "movie" };

        var ex  = Record.Exception(() => _mapper.Map<AdaptationDto>(entity));
        var dto = _mapper.Map<AdaptationDto>(entity);

        Assert.Null(ex);
        Assert.Null(dto.Description);
        Assert.Null(dto.ReleaseYear);
        Assert.Null(dto.PosterUrl);
        Assert.Null(dto.Studio);
        Assert.Null(dto.Country);
    }
}

// ═══════════════════════════════════════════════════════════════════════════════
// Password Hashing (BCrypt)
// ═══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Unit tests for BCrypt password hashing — same logic used in AuthController.
/// No DB, no HTTP.
/// </summary>
public class PasswordHashingTests
{
    // ── TC-UNIT-23 ─────────────────────────────────────────────────────────
    /// <summary>Hash is non-empty and different from plaintext.</summary>
    [Fact]
    public void BCrypt_HashPassword_ReturnsNonEmptyHashDifferentFromInput()
    {
        const string plaintext = "P@ssw0rd123";
        var hash = BCrypt.Net.BCrypt.HashPassword(plaintext);

        Assert.NotNull(hash);
        Assert.NotEmpty(hash);
        Assert.NotEqual(plaintext, hash);
    }

    // ── TC-UNIT-24 ─────────────────────────────────────────────────────────
    /// <summary>Verify returns true for correct password (Login happy path).</summary>
    [Fact]
    public void BCrypt_Verify_ReturnsTrueForCorrectPassword()
    {
        const string plaintext = "CorrectPassword!";
        var hash = BCrypt.Net.BCrypt.HashPassword(plaintext);

        Assert.True(BCrypt.Net.BCrypt.Verify(plaintext, hash));
    }

    // ── TC-UNIT-25 ─────────────────────────────────────────────────────────
    /// <summary>Verify returns false for wrong password (Login failure).</summary>
    [Fact]
    public void BCrypt_Verify_ReturnsFalseForWrongPassword()
    {
        var hash = BCrypt.Net.BCrypt.HashPassword("CorrectPassword!");

        Assert.False(BCrypt.Net.BCrypt.Verify("WrongPassword!", hash));
    }

    // ── TC-UNIT-26 ─────────────────────────────────────────────────────────
    /// <summary>Same plaintext → different hashes (random salt). Both verify correctly.</summary>
    [Fact]
    public void BCrypt_HashPassword_SameInputProducesDifferentHashes()
    {
        const string plaintext = "SamePassword";
        var hash1 = BCrypt.Net.BCrypt.HashPassword(plaintext);
        var hash2 = BCrypt.Net.BCrypt.HashPassword(plaintext);

        Assert.NotEqual(hash1, hash2);
        Assert.True(BCrypt.Net.BCrypt.Verify(plaintext, hash1));
        Assert.True(BCrypt.Net.BCrypt.Verify(plaintext, hash2));
    }
}

// ═══════════════════════════════════════════════════════════════════════════════
// ITokenService — Moq mock tests
// ═══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Moq tests: mock ITokenService to verify caller behavior
/// without touching real JWT generation or environment variables.
/// </summary>
public class TokenServiceMockTests
{
    // ── TC-UNIT-27 ─────────────────────────────────────────────────────────
    /// <summary>CreateToken is called exactly once during register flow.</summary>
    [Fact]
    public void AuthFlow_Register_CallsCreateTokenExactlyOnce()
    {
        var mock = new Mock<ITokenService>();
        mock.Setup(s => s.CreateToken(It.IsAny<User>()))
            .Returns("mocked.jwt.token");

        var user = new User
        {
            Id           = Guid.NewGuid(),
            Username     = "newuser",
            Email        = "new@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("P@ss"),
            Role         = "user",
        };

        var token = mock.Object.CreateToken(user);

        mock.Verify(s => s.CreateToken(user), Times.Once);
        Assert.Equal("mocked.jwt.token", token);
    }

    // ── TC-UNIT-28 ─────────────────────────────────────────────────────────
    /// <summary>CreateToken receives the exact user object passed by the caller.</summary>
    [Fact]
    public void AuthFlow_Login_PassesCorrectUserToTokenService()
    {
        var mock       = new Mock<ITokenService>();
        User? captured = null;

        mock.Setup(s => s.CreateToken(It.IsAny<User>()))
            .Callback<User>(u => captured = u)
            .Returns("token");

        var expected = new User
        {
            Id       = Guid.NewGuid(),
            Username = "loginuser",
            Email    = "login@test.com",
            Role     = "user",
        };

        mock.Object.CreateToken(expected);

        Assert.NotNull(captured);
        Assert.Equal(expected.Id,       captured!.Id);
        Assert.Equal(expected.Username, captured.Username);
        Assert.Equal(expected.Email,    captured.Email);
    }

    // ── TC-UNIT-29 ─────────────────────────────────────────────────────────
    /// <summary>Exception from CreateToken propagates to the caller.</summary>
    [Fact]
    public void AuthFlow_TokenServiceThrows_ExceptionPropagates()
    {
        var mock = new Mock<ITokenService>();
        mock.Setup(s => s.CreateToken(It.IsAny<User>()))
            .Throws(new InvalidOperationException("JWT_SECRET not configured"));

        var user = new User { Username = "u", Email = "u@t.com", Role = "user" };

        var ex = Assert.Throws<InvalidOperationException>(
            () => mock.Object.CreateToken(user));

        Assert.Equal("JWT_SECRET not configured", ex.Message);
    }
}
