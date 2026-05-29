using System.Net;
using System.Net.Http.Json;
using Book2Screen.Application.DTOs;
using Xunit;

namespace Book2Screen.Test.IntegrationTests;

public class WorksIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient client;

    public WorksIntegrationTests(CustomWebApplicationFactory<Program> factory)
    {
        this.client = factory.CreateClient();
    }

    [Fact]
    public async Task GetWorks_ReturnsSeededData()
    {
        // Act
        var response = await this.client.GetAsync("/api/v1/Works");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var works = await response.Content.ReadFromJsonAsync<IEnumerable<BookScreenItemDto>>();
        Assert.NotNull(works);
        Assert.NotEmpty(works);
        Assert.Contains(works, w => w.Title.Contains("Dune"));
    }

    [Fact]
    public async Task GetWorkById_ValidId_ReturnsWork()
    {
        // Arrange
        var worksResponse = await this.client.GetAsync("/api/v1/Works");
        var works = await worksResponse.Content.ReadFromJsonAsync<IEnumerable<BookScreenItemDto>>();
        var firstWorkId = works!.First().Id;

        // Act
        var response = await this.client.GetAsync($"/api/v1/Works/{firstWorkId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var work = await response.Content.ReadFromJsonAsync<BookScreenItemDto>();
        Assert.NotNull(work);
        Assert.Equal(firstWorkId, work.Id);
    }

    [Fact]
    public async Task GetWorkById_InvalidId_ReturnsNotFound()
    {
        // Act
        var response = await this.client.GetAsync($"/api/v1/Works/{Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
