// <copyright file="UserProfileController.cs" company="Team 17">
// Copyright (c) Team 17. All rights reserved.
// </copyright>

namespace Book2Screen.API__Web_.Controllers;

using System.Security.Claims;
using Book2Screen.Application.DTOs;
using Book2Screen.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Контролер для керування даними профілю користувача.
/// </summary>
[ApiController]
[Route("api/v1/users/me")]
[Authorize]
[Produces("application/json")]
public class UserProfileController : ControllerBase
{
    private readonly IUserService userService;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserProfileController"/> class.
    /// </summary>
    /// <param name="userService">Сервіс користувачів.</param>
    public UserProfileController(IUserService userService)
    {
        this.userService = userService;
    }

    /// <summary>
    /// Отримати дані профілю поточного користувача.
    /// </summary>
    /// <response code="200">Повертає дані профілю.</response>
    /// <response code="401">Користувач не авторизований.</response>
    /// <response code="404">Користувача не знайдено.</response>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserProfileDto))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyProfile()
    {
        var userId = this.GetUserId();
        var profile = await this.userService.GetProfileAsync(userId);
        return this.Ok(profile);
    }

    /// <summary>
    /// Оновити аватар поточного користувача.
    /// </summary>
    /// <param name="avatarUrl">URL нового аватара.</param>
    /// <response code="200">Аватар успішно оновлено.</response>
    /// <response code="401">Не авторизований.</response>
    /// <response code="404">Користувача не знайдено.</response>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [HttpPost("avatar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateMyAvatar([FromBody] string avatarUrl)
    {
        var userId = this.GetUserId();
        await this.userService.UpdateAvatarAsync(userId, avatarUrl);
        return this.Ok(new { message = "Avatar updated successfully." });
    }

    /// <summary>
    /// Оновити дані профілю поточного користувача.
    /// </summary>
    /// <param name="profileDto">Нові дані профілю.</param>
    /// <response code="200">Профіль успішно оновлено.</response>
    /// <response code="401">Не авторизований.</response>
    /// <response code="404">Користувача не знайдено.</response>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateMyProfile([FromBody] UserProfileDto profileDto)
    {
        var userId = this.GetUserId();
        await this.userService.UpdateProfileAsync(userId, profileDto);
        return this.Ok(new { message = "Profile updated successfully." });
    }

    private Guid GetUserId()
    {
        var userIdClaim = this.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }
}
