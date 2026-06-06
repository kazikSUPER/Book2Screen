// <copyright file="AuthServiceTests.cs" company="Team 17">
// Copyright (c) Team 17. All rights reserved.
// </copyright>

namespace Book2Screen.Tests.Services;

using Book2Screen.Application.DTOs;
using Book2Screen.Application.Interfaces;
using Book2Screen.Application.Services;
using Book2Screen.Domain.Entities;
using Book2Screen.Domain.Exceptions;
using Book2Screen.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

/// <summary>
/// Юніт тести для AuthService.
/// </summary>
public class AuthServiceTests
{
    private readonly Mock<ITokenService> tokenServiceMock;
    private readonly Mock<IEmailService> emailServiceMock;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthServiceTests"/> class.
    /// </summary>
    public AuthServiceTests()
    {
        this.tokenServiceMock = new Mock<ITokenService>();
        this.emailServiceMock = new Mock<IEmailService>();
        this.tokenServiceMock.Setup(t => t.CreateToken(It.IsAny<User>())).Returns("test-token");
    }

    private ApplicationDbContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(dbName)
            .ConfigureWarnings(x => x.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        return new ApplicationDbContext(options);
    }

    // ── LoginAsync ────────────────────────────────────────────────────────────

    /// <summary>
    /// Логін з правильними кредами повертає AuthResponse з токеном.
    /// </summary>
    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsAuthResponse()
    {
        // Arrange
        using var context = this.CreateContext(nameof(this.LoginAsync_ValidCredentials_ReturnsAuthResponse));
        var password = "TestPass123!";
        var user = new User
        {
            Username = "john_doe",
            Email = "john@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Role = "user",
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var service = new AuthService(context, this.tokenServiceMock.Object, this.emailServiceMock.Object);

        // Act
        var result = await service.LoginAsync(new LoginDto { Email = "john@example.com", Password = password });

        // Assert
        result.Should().NotBeNull();
        result!.Token.Should().Be("test-token");
        result.Email.Should().Be("john@example.com");
        result.Username.Should().Be("john_doe");
        result.Role.Should().Be("user");
    }

    /// <summary>
    /// Логін з невірним паролем кидає UnauthorizedException.
    /// </summary>
    [Fact]
    public async Task LoginAsync_WrongPassword_ThrowsUnauthorizedException()
    {
        // Arrange
        using var context = this.CreateContext(nameof(this.LoginAsync_WrongPassword_ThrowsUnauthorizedException));
        var user = new User
        {
            Username = "john_doe",
            Email = "john@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPass123!"),
            Role = "user",
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var service = new AuthService(context, this.tokenServiceMock.Object, this.emailServiceMock.Object);

        // Act & Assert
        await service.Invoking(s => s.LoginAsync(new LoginDto { Email = "john@example.com", Password = "WrongPass!" }))
            .Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Invalid email or password.");
    }

    /// <summary>
    /// Логін з неіснуючим email кидає UnauthorizedException.
    /// </summary>
    [Fact]
    public async Task LoginAsync_NonExistentEmail_ThrowsUnauthorizedException()
    {
        // Arrange
        using var context = this.CreateContext(nameof(this.LoginAsync_NonExistentEmail_ThrowsUnauthorizedException));
        var service = new AuthService(context, this.tokenServiceMock.Object, this.emailServiceMock.Object);

        // Act & Assert
        await service.Invoking(s => s.LoginAsync(new LoginDto { Email = "nobody@example.com", Password = "pass" }))
            .Should().ThrowAsync<UnauthorizedException>();
    }

    // ── RegisterAsync ─────────────────────────────────────────────────────────

    /// <summary>
    /// Реєстрація нового юзера повертає AuthResponse і зберігає в БД.
    /// </summary>
    [Fact]
    public async Task RegisterAsync_NewUser_ReturnsAuthResponseAndSavesToDb()
    {
        // Arrange
        using var context = this.CreateContext(nameof(this.RegisterAsync_NewUser_ReturnsAuthResponseAndSavesToDb));
        var service = new AuthService(context, this.tokenServiceMock.Object, this.emailServiceMock.Object);

        var request = new RegisterRequest
        {
            Username = "new_user",
            Email = "new@example.com",
            Password = "NewPass123!",
        };

        // Act
        var result = await service.RegisterAsync(request);

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be("new@example.com");
        result.Username.Should().Be("new_user");
        result.Role.Should().Be("user");

        var savedUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "new@example.com");
        savedUser.Should().NotBeNull();
        BCrypt.Net.BCrypt.Verify("NewPass123!", savedUser!.PasswordHash).Should().BeTrue();
    }

    /// <summary>
    /// Реєстрація з дублікатом email кидає ConflictException.
    /// </summary>
    [Fact]
    public async Task RegisterAsync_DuplicateEmail_ThrowsConflictException()
    {
        // Arrange
        using var context = this.CreateContext(nameof(this.RegisterAsync_DuplicateEmail_ThrowsConflictException));
        context.Users.Add(new User
        {
            Username = "existing_user",
            Email = "existing@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("pass"),
            Role = "user",
        });
        await context.SaveChangesAsync();

        var service = new AuthService(context, this.tokenServiceMock.Object, this.emailServiceMock.Object);

        // Act & Assert
        await service.Invoking(s => s.RegisterAsync(new RegisterRequest
        {
            Username = "new_nick",
            Email = "existing@example.com",
            Password = "NewPass123!",
        }))
        .Should().ThrowAsync<ConflictException>()
        .WithMessage("*already exists*");
    }

    /// <summary>
    /// Реєстрація з дублікатом нікнейму кидає ConflictException.
    /// </summary>
    [Fact]
    public async Task RegisterAsync_DuplicateNickname_ThrowsConflictException()
    {
        // Arrange
        using var context = this.CreateContext(nameof(this.RegisterAsync_DuplicateNickname_ThrowsConflictException));
        context.Users.Add(new User
        {
            Username = "existing_nick",
            Email = "other@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("pass"),
            Role = "user",
        });
        await context.SaveChangesAsync();

        var service = new AuthService(context, this.tokenServiceMock.Object, this.emailServiceMock.Object);

        await service.Invoking(s => s.RegisterAsync(new RegisterRequest
        {
            Username = "existing_nick",
            Email = "new@example.com",
            Password = "NewPass123!",
        }))
        .Should().ThrowAsync<ConflictException>();
    }

    // ── ForgotPasswordAsync ───────────────────────────────────────────────────

    /// <summary>
    /// ForgotPassword для існуючого юзера зберігає токен і надсилає email.
    /// </summary>
    [Fact]
    public async Task ForgotPasswordAsync_ExistingUser_SavesTokenAndSendsEmail()
    {
        // Arrange
        using var context = this.CreateContext(nameof(this.ForgotPasswordAsync_ExistingUser_SavesTokenAndSendsEmail));
        context.Users.Add(new User
        {
            Username = "john_doe",
            Email = "john@example.com",
            PasswordHash = "hash",
            Role = "user",
        });
        await context.SaveChangesAsync();

        this.emailServiceMock
            .Setup(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var service = new AuthService(context, this.tokenServiceMock.Object, this.emailServiceMock.Object);

        // Act
        var result = await service.ForgotPasswordAsync(new ForgotPasswordRequest { Email = "john@example.com" });

        // Assert
        result.Should().BeTrue();
        var token = await context.PasswordResetTokens.FirstOrDefaultAsync(t => t.Email == "john@example.com");
        token.Should().NotBeNull();
        token!.IsUsed.Should().BeFalse();
        token.ExpiryTime.Should().BeAfter(DateTime.UtcNow);

        this.emailServiceMock.Verify(
            e => e.SendEmailAsync("john@example.com", It.IsAny<string>(), It.IsAny<string>()),
            Times.Once);
    }

    /// <summary>
    /// ForgotPassword для неіснуючого юзера повертає true але не надсилає email.
    /// </summary>
    [Fact]
    public async Task ForgotPasswordAsync_NonExistentUser_ReturnsTrueWithoutEmail()
    {
        // Arrange
        using var context = this.CreateContext(nameof(this.ForgotPasswordAsync_NonExistentUser_ReturnsTrueWithoutEmail));
        var service = new AuthService(context, this.tokenServiceMock.Object, this.emailServiceMock.Object);

        // Act
        var result = await service.ForgotPasswordAsync(new ForgotPasswordRequest { Email = "nobody@example.com" });

        // Assert
        result.Should().BeTrue();
        this.emailServiceMock.Verify(
            e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    // ── ResetPasswordAsync ────────────────────────────────────────────────────

    /// <summary>
    /// ResetPassword з валідним кодом змінює пароль і позначає токен як використаний. (BUG-039)
    /// </summary>
    [Fact]
    public async Task ResetPasswordAsync_ValidCode_ChangesPasswordAndMarksTokenUsed()
    {
        // Arrange
        using var context = this.CreateContext(nameof(this.ResetPasswordAsync_ValidCode_ChangesPasswordAndMarksTokenUsed));
        var user = new User
        {
            Username = "john_doe",
            Email = "john@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("OldPass123!"),
            Role = "user",
        };
        context.Users.Add(user);
        context.PasswordResetTokens.Add(new PasswordResetToken
        {
            Email = "john@example.com",
            Code = "123456",
            ExpiryTime = DateTime.UtcNow.AddMinutes(15),
            IsUsed = false,
        });
        await context.SaveChangesAsync();

        var service = new AuthService(context, this.tokenServiceMock.Object, this.emailServiceMock.Object);

        // Act
        var result = await service.ResetPasswordAsync(new ResetPasswordRequest
        {
            Email = "john@example.com",
            Code = "123456",
            NewPassword = "NewPass456!",
        });

        // Assert
        result.Should().NotBeNull();

        var updatedUser = await context.Users.FirstAsync(u => u.Email == "john@example.com");
        BCrypt.Net.BCrypt.Verify("NewPass456!", updatedUser.PasswordHash).Should().BeTrue();

        var usedToken = await context.PasswordResetTokens.FirstAsync(t => t.Email == "john@example.com");
        usedToken.IsUsed.Should().BeTrue();
    }

    /// <summary>
    /// ResetPassword з невірним кодом повертає null. (BUG-039)
    /// </summary>
    [Fact]
    public async Task ResetPasswordAsync_InvalidCode_ReturnsNull()
    {
        // Arrange
        using var context = this.CreateContext(nameof(this.ResetPasswordAsync_InvalidCode_ReturnsNull));
        context.Users.Add(new User
        {
            Username = "john_doe",
            Email = "john@example.com",
            PasswordHash = "hash",
            Role = "user",
        });
        context.PasswordResetTokens.Add(new PasswordResetToken
        {
            Email = "john@example.com",
            Code = "123456",
            ExpiryTime = DateTime.UtcNow.AddMinutes(15),
            IsUsed = false,
        });
        await context.SaveChangesAsync();

        var service = new AuthService(context, this.tokenServiceMock.Object, this.emailServiceMock.Object);

        // Act
        var result = await service.ResetPasswordAsync(new ResetPasswordRequest
        {
            Email = "john@example.com",
            Code = "999999",
            NewPassword = "NewPass456!",
        });

        // Assert
        result.Should().BeNull();
    }

    /// <summary>
    /// ResetPassword з протермінованим токеном повертає null.
    /// </summary>
    [Fact]
    public async Task ResetPasswordAsync_ExpiredToken_ReturnsNull()
    {
        // Arrange
        using var context = this.CreateContext(nameof(this.ResetPasswordAsync_ExpiredToken_ReturnsNull));
        context.Users.Add(new User
        {
            Username = "john_doe",
            Email = "john@example.com",
            PasswordHash = "hash",
            Role = "user",
        });
        context.PasswordResetTokens.Add(new PasswordResetToken
        {
            Email = "john@example.com",
            Code = "123456",
            ExpiryTime = DateTime.UtcNow.AddMinutes(-1),
            IsUsed = false,
        });
        await context.SaveChangesAsync();

        var service = new AuthService(context, this.tokenServiceMock.Object, this.emailServiceMock.Object);

        // Act
        var result = await service.ResetPasswordAsync(new ResetPasswordRequest
        {
            Email = "john@example.com",
            Code = "123456",
            NewPassword = "NewPass456!",
        });

        // Assert
        result.Should().BeNull();
    }

    /// <summary>
    /// ResetPassword з вже використаним токеном повертає null. (BUG-039)
    /// </summary>
    [Fact]
    public async Task ResetPasswordAsync_AlreadyUsedToken_ReturnsNull()
    {
        // Arrange
        using var context = this.CreateContext(nameof(this.ResetPasswordAsync_AlreadyUsedToken_ReturnsNull));
        context.Users.Add(new User
        {
            Username = "john_doe",
            Email = "john@example.com",
            PasswordHash = "hash",
            Role = "user",
        });
        context.PasswordResetTokens.Add(new PasswordResetToken
        {
            Email = "john@example.com",
            Code = "123456",
            ExpiryTime = DateTime.UtcNow.AddMinutes(15),
            IsUsed = true,
        });
        await context.SaveChangesAsync();

        var service = new AuthService(context, this.tokenServiceMock.Object, this.emailServiceMock.Object);

        // Act
        var result = await service.ResetPasswordAsync(new ResetPasswordRequest
        {
            Email = "john@example.com",
            Code = "123456",
            NewPassword = "NewPass456!",
        });

        // Assert
        result.Should().BeNull();
    }
}
