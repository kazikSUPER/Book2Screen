namespace Book2Screen.API__Web_.Controllers;

using Book2Screen.Application.DTOs;
using Book2Screen.Application.Interfaces;
using Book2Screen.Domain.Entities;
using Book2Screen.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Контролер для керування автентифікацією та реєстрацією користувачів.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly ITokenService tokenService;
    private readonly ApplicationDbContext context;

    public AuthController(ITokenService tokenService, ApplicationDbContext context)
    {
        this.tokenService = tokenService;
        this.context = context;
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
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        try
        {
            var user = await this.context.Users.FirstOrDefaultAsync(u => u.Username == loginDto.Username);
            if (user == null)
            {
                return this.Unauthorized("Invalid username or password.");
            }

            if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            {
                return this.Unauthorized("Invalid username or password.");
            }

            var token = this.tokenService.CreateToken(user);
            return this.Ok(new { token });
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
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest registerRequest)
    {
        var user = await this.context.Users.FirstOrDefaultAsync(u =>
            u.Username == registerRequest.Username || u.Email == registerRequest.Email);

        if (user != null)
        {
            return this.Unauthorized("User with this username or email already exists.");
        }

        user = new User
        {
            Username = registerRequest.Username,
            Email = registerRequest.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerRequest.Password),
        };

        await this.context.Users.AddAsync(user);
        await this.context.SaveChangesAsync();

        var token = this.tokenService.CreateToken(user);
        return this.Ok(new { token });
    }
}
