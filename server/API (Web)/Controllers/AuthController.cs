// <copyright file="AuthController.cs" company="Team 17">
// Copyright (c) Team 17. All rights reserved.
// </copyright>

namespace Book2Screen.API__Web_.Controllers;

using Book2Screen.Application.DTOs;
using Book2Screen.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Контролер для керування автентифікацією та реєстрацією користувачів.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService authService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthController"/> class.
    /// </summary>
    /// <param name="authService">Сервіс автентифікації.</param>
    public AuthController(IAuthService authService)
    {
        this.authService = authService;
    }

    /// <summary>
    /// Авторизація користувача в системі.
    /// </summary>
    /// <param name="loginDto">Дані для входу (логін та пароль).</param>
    /// <returns>Повертає JWT токен у разі успіху.</returns>
    /// <response code="200">Успішний вхід. Повертає об'єкт з токеном.</response>
    /// <response code="401">Невірне ім'я користувача або пароль.</response>
    /// <response code="400">Помилка при обробці запиту.</response>
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AuthResponse))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        try
        {
            var response = await this.authService.LoginAsync(loginDto);
            if (response == null)
            {
                return this.Unauthorized("Invalid username or password.");
            }

            return this.Ok(response);
        }
        catch (Exception ex)
        {
            return this.BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Реєстрація нового користувача.
    /// </summary>
    /// <param name="registerRequest">Дані для реєстрації (логін, пошта, пароль).</param>
    /// <returns>Повертає JWT токен для нового користувача.</returns>
    /// <response code="200">Користувача успішно створено. Повертає токен.</response>
    /// <response code="409">Користувач з таким логіном або поштою вже існує.</response>
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AuthResponse))]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest registerRequest)
    {
        var response = await this.authService.RegisterAsync(registerRequest);

        if (response == null)
        {
            return this.Conflict("User with this username or email already exists.");
        }

        return this.Ok(response);
    }

    /// <summary>
    /// Запит на відновлення паролю.
    /// </summary>
    /// <param name="request">Запит з Email.</param>
    /// <returns>Повертає 200 OK.</returns>
    [HttpPost("password-reset")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        await this.authService.ForgotPasswordAsync(request);
        return this.Ok(new { message = "If the email exists, a reset code has been sent." });
    }

    /// <summary>
    /// Перевірка коду відновлення.
    /// </summary>
    /// <param name="request">Запит з Email та кодом.</param>
    /// <returns>Повертає 200 OK, якщо код вірний.</returns>
    [HttpPost("verify-code")]
    public async Task<IActionResult> VerifyCode([FromBody] VerifyCodeRequest request)
    {
        var isValid = await this.authService.VerifyResetCodeAsync(request);
        if (!isValid)
        {
            return this.BadRequest("Invalid or expired code.");
        }

        return this.Ok(new { message = "Code verified successfully." });
    }

    /// <summary>
    /// Скидання паролю на новий.
    /// </summary>
    /// <param name="request">Запит з Email, кодом та новим паролем.</param>
    /// <returns>Повертає 200 OK у разі успіху.</returns>
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var success = await this.authService.ResetPasswordAsync(request);
        if (!success)
        {
            return this.BadRequest("Invalid code or email.");
        }

        return this.Ok(new { message = "Password has been reset successfully." });
    }
}
