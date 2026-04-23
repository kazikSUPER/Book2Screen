namespace Book2Screen.API__Web_.Controllers;

using Book2Screen.Application.DTOs;
using Book2Screen.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Контролер для керування автентифікацією та реєстрацією користувачів.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService authService;

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
            var token = await this.authService.LoginAsync(loginDto);
            if (token == null)
            {
                return this.Unauthorized("Invalid username or password.");
            }

            return this.Ok(new AuthResponse { Token = token });
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
    /// <response code="401">Користувач з таким логіном або поштою вже існує.</response>
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AuthResponse))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest registerRequest)
    {
        var token = await this.authService.RegisterAsync(registerRequest);

        if (token == null)
        {
            return this.Unauthorized("User with this username or email already exists.");
        }

        return this.Ok(new AuthResponse { Token = token });
    }
}
