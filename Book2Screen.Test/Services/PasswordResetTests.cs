// Book2Screen.Tests/Services/PasswordResetTests.cs
namespace Book2Screen.Tests.Services;

using Book2Screen.Application.DTOs;
using Book2Screen.Application.Interfaces;
using Book2Screen.Application.Services;
using Book2Screen.Domain.Entities;
using Book2Screen.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

public class PasswordResetTests
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly IAuthService _authService;

    public PasswordResetTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _tokenServiceMock = new Mock<ITokenService>();

        _authService = new AuthService(_context, _tokenServiceMock.Object);
    }

    [Fact]
    public async Task ForgotPasswordAsync_ShouldReturnTrueAndGenerateCode_WhenUserExists()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hash"
        };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var request = new ForgotPasswordRequest { Email = "test@example.com" };

        // Act
        var result = await _authService.ForgotPasswordAsync(request);

        // Assert
        Assert.True(result);
        var token = await _context.PasswordResetTokens.FirstOrDefaultAsync(t => t.Email == "test@example.com");
        Assert.NotNull(token);
        Assert.NotNull(token.Code);
        Assert.False(token.IsUsed);
        Assert.True(token.ExpiryTime > DateTime.UtcNow);
    }

    [Fact]
    public async Task ForgotPasswordAsync_ShouldReturnTrueButNotGenerateCode_WhenUserDoesNotExist()
    {
        // Arrange
        var request = new ForgotPasswordRequest { Email = "nonexistent@example.com" };

        // Act
        var result = await _authService.ForgotPasswordAsync(request);

        // Assert
        Assert.True(result); // Returns true for security reasons
        var token = await _context.PasswordResetTokens.FirstOrDefaultAsync(t => t.Email == "nonexistent@example.com");
        Assert.Null(token);
    }

    [Fact]
    public async Task VerifyResetCodeAsync_ShouldReturnTrue_WhenCodeIsValid()
    {
        // Arrange
        var token = new PasswordResetToken
        {
            Email = "test@example.com",
            Code = "123456",
            ExpiryTime = DateTime.UtcNow.AddMinutes(10),
            IsUsed = false
        };
        await _context.PasswordResetTokens.AddAsync(token);
        await _context.SaveChangesAsync();

        var request = new VerifyCodeRequest { Email = "test@example.com", Code = "123456" };

        // Act
        var result = await _authService.VerifyResetCodeAsync(request);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task VerifyResetCodeAsync_ShouldReturnFalse_WhenCodeIsExpired()
    {
        // Arrange
        var token = new PasswordResetToken
        {
            Email = "test@example.com",
            Code = "123456",
            ExpiryTime = DateTime.UtcNow.AddMinutes(-5), // Expired
            IsUsed = false
        };
        await _context.PasswordResetTokens.AddAsync(token);
        await _context.SaveChangesAsync();

        var request = new VerifyCodeRequest { Email = "test@example.com", Code = "123456" };

        // Act
        var result = await _authService.VerifyResetCodeAsync(request);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task VerifyResetCodeAsync_ShouldReturnFalse_WhenCodeIsUsed()
    {
        // Arrange
        var token = new PasswordResetToken
        {
            Email = "test@example.com",
            Code = "123456",
            ExpiryTime = DateTime.UtcNow.AddMinutes(10),
            IsUsed = true // Already used
        };
        await _context.PasswordResetTokens.AddAsync(token);
        await _context.SaveChangesAsync();

        var request = new VerifyCodeRequest { Email = "test@example.com", Code = "123456" };

        // Act
        var result = await _authService.VerifyResetCodeAsync(request);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ResetPasswordAsync_ShouldResetPasswordAndMarkTokenAsUsed_WhenValid()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "oldhash"
        };
        await _context.Users.AddAsync(user);

        var token = new PasswordResetToken
        {
            Email = "test@example.com",
            Code = "123456",
            ExpiryTime = DateTime.UtcNow.AddMinutes(10),
            IsUsed = false
        };
        await _context.PasswordResetTokens.AddAsync(token);
        await _context.SaveChangesAsync();

        var request = new ResetPasswordRequest
        {
            Email = "test@example.com",
            Code = "123456",
            NewPassword = "newPassword123!"
        };

        // Act
        var result = await _authService.ResetPasswordAsync(request);

        // Assert
        Assert.True(result);
        var updatedUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == "test@example.com");
        Assert.NotEqual("oldhash", updatedUser!.PasswordHash);
        Assert.True(BCrypt.Net.BCrypt.Verify("newPassword123!", updatedUser.PasswordHash));

        var updatedToken = await _context.PasswordResetTokens.FirstOrDefaultAsync(t => t.Id == token.Id);
        Assert.True(updatedToken!.IsUsed);
    }
}
