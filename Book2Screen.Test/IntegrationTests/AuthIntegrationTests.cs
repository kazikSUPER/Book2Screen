// <copyright file="AuthIntegrationTests.cs" company="Team 17">
// Copyright (c) Team 17. All rights reserved.
// </copyright>

namespace Book2Screen.Test.IntegrationTests;

using System.Net;
using System.Net.Http.Json;
using Book2Screen.Application.DTOs;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

/// <summary>
/// Інтеграційні тести для автентифікації та Rate Limiting.
/// </summary>
public class AuthIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient client;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthIntegrationTests"/> class.
    /// </summary>
    /// <param name="factory">Фабрика для створення тестового сервера.</param>
    public AuthIntegrationTests(CustomWebApplicationFactory<Program> factory)
    {
        this.client = factory.CreateClient();
    }

    /// <summary>
    /// Перевіряє роботу Rate Limiter при перевищенні ліміту запитів (10 на хвилину).
    /// </summary>
    [Fact]
    public async Task Login_RateLimiting_ShouldReturn429TooManyRequests()
    {
        // Arrange
        var loginDto = new LoginDto { Email = "test@example.com", Password = "Password123" };
        
        // Act: Відправляємо 11 запитів (ліміт 10)
        HttpResponseMessage lastResponse = null!;
        for (int i = 0; i < 12; i++)
        {
            lastResponse = await this.client.PostAsJsonAsync("/api/v1/auth/login", loginDto);
        }

        // Assert
        Assert.Equal(HttpStatusCode.TooManyRequests, lastResponse.StatusCode);
    }

    /// <summary>
    /// Перевіряє валідацію складного пароля при реєстрації.
    /// </summary>
    [Theory]
    [InlineData("simple")] // Занадто короткий
    [InlineData("password123")] // Немає великої літери
    [InlineData("PASSWORD123")] // Немає малої літери (опціонально, але регулярка вимагає цифру і велику)
    [InlineData("Password")] // Немає цифри
    public async Task Register_WeakPassword_ShouldReturn400BadRequest(string weakPassword)
    {
        // Arrange
        var request = new RegisterRequest 
        { 
            Username = "testuser", 
            Email = "test@example.com", 
            Password = weakPassword 
        };

        // Act
        var response = await this.client.PostAsJsonAsync("/api/v1/auth/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
