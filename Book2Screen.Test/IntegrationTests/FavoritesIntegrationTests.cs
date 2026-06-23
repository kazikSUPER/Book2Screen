// <copyright file="FavoritesIntegrationTests.cs" company="Team 17">
// Copyright (c) Team 17. All rights reserved.
// </copyright>

namespace Book2Screen.Test.IntegrationTests;

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Book2Screen.Application.DTOs;
using Xunit;

/// <summary>
/// Інтеграційні тести для роботи з обраним.
/// </summary>
public class FavoritesIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient client;

    /// <summary>
    /// Initializes a new instance of the <see cref="FavoritesIntegrationTests"/> class.
    /// </summary>
    /// <param name="factory">Фабрика для створення тестового сервера.</param>
    public FavoritesIntegrationTests(CustomWebApplicationFactory<Program> factory)
    {
        this.client = factory.CreateClient();
    }

    /// <summary>
    /// Перевіряє, що повторне додавання в обране повертає 409 Conflict.
    /// </summary>
    [Fact]
    public async Task AddToFavorites_Duplicate_ShouldReturn409Conflict()
    {
        // Arrange: 1. Авторизуємось (дані з DbSeeder.cs)
        var loginDto = new LoginDto { Email = "admin@book2screen.com", Password = "Admin123!" };
        var authRes = await this.client.PostAsJsonAsync("/api/v1/auth/login", loginDto);
        var authData = await authRes.Content.ReadFromJsonAsync<AuthResponse>();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authData!.Token);

        // 2. Беремо будь-який твір
        var worksRes = await this.client.GetAsync("/api/v1/works");
        var works = await worksRes.Content.ReadFromJsonAsync<IEnumerable<BookScreenItemDto>>();
        var workId = works!.First().Id;

        // 3. Видаляємо його з обраного про всяк випадок (чистий стан)
        await this.client.DeleteAsync($"/api/v1/favorites/{workId}?kind=favorite");

        // 4. Додаємо вперше
        var favReq = new FavoriteRequest { WorkId = workId, Kind = "favorite" };
        var res1 = await this.client.PostAsJsonAsync("/api/v1/favorites", favReq);
        Assert.Equal(HttpStatusCode.OK, res1.StatusCode);

        // Act: 5. Додаємо вдруге
        var res2 = await this.client.PostAsJsonAsync("/api/v1/favorites", favReq);

        // Assert: Має бути 409 Conflict
        Assert.Equal(HttpStatusCode.Conflict, res2.StatusCode);
    }
}
