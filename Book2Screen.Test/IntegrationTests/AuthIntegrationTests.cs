using System.Net;
using System.Net.Http.Json;
using Book2Screen.Application.DTOs;
using Xunit;

namespace Book2Screen.Test.IntegrationTests;

public class AuthIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient client;
    private readonly CustomWebApplicationFactory<Program> factory;

    public AuthIntegrationTests(CustomWebApplicationFactory<Program> factory)
    {
        this.factory = factory;
        this.client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_NewUser_ReturnsSuccess()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Nickname = "testuser_int",
            Email = "test_int@example.com",
            Password = "Password123!"
        };

        // Act
        var response = await this.client.PostAsJsonAsync("/api/v1/Auth/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(authResponse);
        Assert.NotNull(authResponse.Token);
        Assert.Equal(request.Email, authResponse.Email);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsToken()
    {
        // Arrange
        var registerRequest = new RegisterRequest
        {
            Nickname = "login_test",
            Email = "login_test@example.com",
            Password = "Password123!"
        };
        await this.client.PostAsJsonAsync("/api/v1/Auth/register", registerRequest);

        var loginDto = new LoginDto
        {
            Email = registerRequest.Email,
            Password = registerRequest.Password
        };

        // Act
        var response = await this.client.PostAsJsonAsync("/api/v1/Auth/login", loginDto);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(authResponse);
        Assert.NotNull(authResponse.Token);
    }

    [Fact]
    public async Task Register_DuplicateUser_ReturnsConflict()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Nickname = "duplicate",
            Email = "duplicate@example.com",
            Password = "Password123!"
        };
        await this.client.PostAsJsonAsync("/api/v1/Auth/register", request);

        // Act
        var response = await this.client.PostAsJsonAsync("/api/v1/Auth/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }
}
