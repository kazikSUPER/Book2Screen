// <copyright file="InfrastructureIntegrationTests.cs" company="Team 17">
// Copyright (c) Team 17. All rights reserved.
// </copyright>

namespace Book2Screen.Test.IntegrationTests;

using System.Net;
using Xunit;

/// <summary>
/// Інтеграційні тести для інфраструктурних компонентів (Health Checks, Rate Limiting).
/// </summary>
public class InfrastructureIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient client;

    /// <summary>
    /// Initializes a new instance of the <see cref="InfrastructureIntegrationTests"/> class.
    /// </summary>
    /// <param name="factory">Фабрика для створення тестового сервера.</param>
    public InfrastructureIntegrationTests(CustomWebApplicationFactory<Program> factory)
    {
        this.client = factory.CreateClient();
    }

    /// <summary>
    /// Перевіряє доступність ендпоїнта Health Check.
    /// В тестовому середовищі без реальної БД ми очікуємо 503 Service Unavailable,
    /// але перевіряємо, що сам механізм опитування працює і повертає JSON.
    /// </summary>
    [Fact]
    public async Task HealthCheck_ShouldReturnJson()
    {
        // Act
        var response = await this.client.GetAsync("/health");

        // Assert (В In-Memory середовищі NpgSql та Disk checks будуть Unhealthy/Degraded)
        Assert.True(response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.ServiceUnavailable);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("status", content);
    }
}
