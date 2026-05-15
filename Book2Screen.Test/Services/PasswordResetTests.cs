// <copyright file="PasswordResetTests.cs" company="Team 17">
// Copyright (c) Team 17. All rights reserved.
// </copyright>

namespace Book2Screen.Tests.Services;

using Book2Screen.Application.DTOs;
using Book2Screen.Application.Interfaces;
using Book2Screen.Application.Services;
using Book2Screen.Domain.Entities;
using Book2Screen.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

/// <summary>
/// Unit tests for password reset functionality.
/// </summary>
public class PasswordResetTests
{
    private readonly ApplicationDbContext context;
    private readonly Mock<ITokenService> tokenServiceMock;
    private readonly Mock<IEmailService> emailServiceMock;
    private readonly IAuthService authService;

    /// <summary>
    /// Initializes a new instance of the <see cref="PasswordResetTests"/> class.
    /// </summary>
    public PasswordResetTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        this.context = new ApplicationDbContext(options);
        this.tokenServiceMock = new Mock<ITokenService>();
        this.tokenServiceMock.Setup(t => t.CreateToken(It.IsAny<User>())).Returns("fake-jwt-token");
        this.emailServiceMock = new Mock<IEmailService>();

        this.authService = new AuthService(this.context, this.tokenServiceMock.Object, this.emailServiceMock.Object);
    }

    /// <summary>
    /// Checks that ForgotPasswordAsync generates a code and calls email service when user exists.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Fact]
    public async Task ForgotPasswordAsync_ShouldReturnTrueAndSendEmail_WhenUserExists()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hash",
        };
        await this.context.Users.AddAsync(user);
        await this.context.SaveChangesAsync();

        var request = new ForgotPasswordRequest { Email = "test@example.com" };

        // Act
        var result = await this.authService.ForgotPasswordAsync(request);

        // Assert
        Assert.True(result);
        var token = await this.context.PasswordResetTokens.FirstOrDefaultAsync(t => t.Email == "test@example.com");
        Assert.NotNull(token);
        Assert.NotNull(token.Code);
        Assert.False(token.IsUsed);
        this.emailServiceMock.Verify(
            s => s.SendEmailAsync(
                It.Is<string>(e => e == "test@example.com"),
                It.IsAny<string>(),
                It.Is<string>(b => b.Contains(token.Code))),
            Times.Once);
    }

    /// <summary>
    /// Checks that ForgotPasswordAsync returns true but does nothing when user does not exist.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Fact]
    public async Task ForgotPasswordAsync_ShouldReturnTrueButNotSendEmail_WhenUserDoesNotExist()
    {
        // Arrange
        var request = new ForgotPasswordRequest { Email = "nonexistent@example.com" };

        // Act
        var result = await this.authService.ForgotPasswordAsync(request);

        // Assert
        Assert.True(result);
        var token = await this.context.PasswordResetTokens.FirstOrDefaultAsync(t => t.Email == "nonexistent@example.com");
        Assert.Null(token);
        this.emailServiceMock.Verify(s => s.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    /// <summary>
    /// Checks that VerifyResetCodeAsync returns true for valid code.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Fact]
    public async Task VerifyResetCodeAsync_ShouldReturnTrue_WhenCodeIsValid()
    {
        // Arrange
        var token = new PasswordResetToken
        {
            Email = "test@example.com",
            Code = "123456",
            ExpiryTime = DateTime.UtcNow.AddMinutes(10),
            IsUsed = false,
        };
        await this.context.PasswordResetTokens.AddAsync(token);
        await this.context.SaveChangesAsync();

        var request = new VerifyCodeRequest { Email = "test@example.com", Code = "123456" };

        // Act
        var result = await this.authService.VerifyResetCodeAsync(request);

        // Assert
        Assert.True(result);
    }

    /// <summary>
    /// Checks that ResetPasswordAsync updates the password.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Fact]
    public async Task ResetPasswordAsync_ShouldUpdatePassword_WhenValid()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("OldPass"),
        };
        await this.context.Users.AddAsync(user);

        var token = new PasswordResetToken
        {
            Email = "test@example.com",
            Code = "123456",
            ExpiryTime = DateTime.UtcNow.AddMinutes(10),
            IsUsed = false,
        };
        await this.context.PasswordResetTokens.AddAsync(token);
        await this.context.SaveChangesAsync();

        var request = new ResetPasswordRequest
        {
            Email = "test@example.com",
            Code = "123456",
            NewPassword = "NewPass123!",
        };

        // Act
        var result = await this.authService.ResetPasswordAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Token);
        var updatedUser = await this.context.Users.FirstOrDefaultAsync(u => u.Email == "test@example.com");
        Assert.True(BCrypt.Net.BCrypt.Verify("NewPass123!", updatedUser!.PasswordHash));
        var updatedToken = await this.context.PasswordResetTokens.FirstOrDefaultAsync(t => t.Id == token.Id);
        Assert.True(updatedToken!.IsUsed);
    }
}
