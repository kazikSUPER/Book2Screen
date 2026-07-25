// <copyright file="PasswordResetIntegrationTests.cs" company="Team 17">
// Copyright (c) Team 17. All rights reserved.
// </copyright>

namespace Book2Screen.Test.IntegrationTests;

using System.Net;
using System.Net.Http.Json;
using Book2Screen.Application.DTOs;
using Book2Screen.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

/// <summary>
/// Інтеграційні тести для перевірки циклу відновлення пароля.
/// </summary>
public class PasswordResetIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient client;
    private readonly CustomWebApplicationFactory<Program> factory;

    /// <summary>
    /// Initializes a new instance of the <see cref="PasswordResetIntegrationTests"/> class.
    /// </summary>
    /// <param name="factory">Фабрика для створення тестового сервера.</param>
    public PasswordResetIntegrationTests(CustomWebApplicationFactory<Program> factory)
    {
        this.factory = factory;
        this.client = factory.CreateClient();
    }

    /// <summary>
    /// Перевіряє повний цикл відновлення пароля: Forgot -> Verify -> Reset.
    /// </summary>
    [Fact]
    public async Task PasswordReset_FullCycle_ShouldSucceed()
    {
        // Arrange
        var email = "john@example.com";
        var forgotReq = new ForgotPasswordRequest { Email = email };

        // 1. Запит на скидання (Forgot Password)
        var forgotRes = await this.client.PostAsJsonAsync("/api/v1/auth/password-reset", forgotReq);
        Assert.Equal(HttpStatusCode.OK, forgotRes.StatusCode);

        // 2. Отримуємо код з БД (через сервіс-провайдер тесту)
        string code;
        using (var scope = this.factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var token = db.PasswordResetTokens.FirstOrDefault(t => t.Email == email);
            Assert.NotNull(token);
            code = token.Code;
        }

        // 3. Перевірка коду (Verify Code)
        var verifyReq = new VerifyCodeRequest { Email = email, Code = code };
        var verifyRes = await this.client.PostAsJsonAsync("/api/v1/auth/verify-code", verifyReq);
        Assert.Equal(HttpStatusCode.OK, verifyRes.StatusCode);

        // 4. Скидання пароля (Reset Password)
        var resetReq = new ResetPasswordRequest 
        { 
            Email = email, 
            Code = code, 
            NewPassword = "NewStrongPassword123!" 
        };
        var resetRes = await this.client.PostAsJsonAsync("/api/v1/auth/reset-password", resetReq);

        // Assert: BUG-039 каже, що тут може бути 400. Перевіряємо.
        Assert.Equal(HttpStatusCode.OK, resetRes.StatusCode);
        var authData = await resetRes.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(authData!.Token);

        // 5. Перевірка логіну з новим паролем
        var loginDto = new LoginDto { Email = email, Password = "NewStrongPassword123!" };
        var loginRes = await this.client.PostAsJsonAsync("/api/v1/auth/login", loginDto);
        Assert.Equal(HttpStatusCode.OK, loginRes.StatusCode);
    }
}
