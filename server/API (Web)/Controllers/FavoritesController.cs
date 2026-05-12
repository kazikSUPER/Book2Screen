// <copyright file="FavoritesController.cs" company="Team 17">
// Copyright (c) Team 17. All rights reserved.
// </copyright>

namespace Book2Screen.API__Web_.Controllers;

using System.Security.Claims;
using Book2Screen.Application.DTOs;
using Book2Screen.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Контролер для керування списком обраних творів.
/// </summary>
[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class FavoritesController : ControllerBase
{
    private readonly IFavoriteService favoriteService;

    /// <summary>
    /// Initializes a new instance of the <see cref="FavoritesController"/> class.
    /// </summary>
    /// <param name="favoriteService">Сервіс обраного.</param>
    public FavoritesController(IFavoriteService favoriteService)
    {
        this.favoriteService = favoriteService;
    }

    /// <summary>
    /// Отримує список обраних творів поточного користувача.
    /// </summary>
    /// <returns>Список творів.</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<BookScreenItemDto>))]
    public async Task<IActionResult> GetFavorites()
    {
        var userId = this.GetUserId();
        if (userId == Guid.Empty)
        {
            return this.Unauthorized();
        }

        var favorites = await this.favoriteService.GetUserFavoritesAsync(userId);
        return this.Ok(favorites);
    }

    /// <summary>
    /// Додає твір в обране.
    /// </summary>
    /// <param name="request">Запит з ID твору.</param>
    /// <returns>Статус операції.</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddToFavorites([FromBody] FavoriteRequest request)
    {
        var userId = this.GetUserId();
        if (userId == Guid.Empty)
        {
            return this.Unauthorized();
        }

        if (!request.WorkId.HasValue)
        {
            return this.BadRequest("WorkId is required.");
        }

        var success = await this.favoriteService.AddToFavoritesAsync(userId, request.WorkId.Value);
        if (!success)
        {
            return this.BadRequest("Failed to add to favorites.");
        }

        return this.Ok(new { message = "Added to favorites." });
    }

    /// <summary>
    /// Видалити твір з обраного.
    /// </summary>
    /// <param name="workId">ID твору.</param>
    /// <returns>Статус операції.</returns>
    [HttpDelete("{workId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RemoveFromFavorites(Guid workId)
    {
        var userId = this.GetUserId();
        if (userId == Guid.Empty)
        {
            return this.Unauthorized();
        }

        var success = await this.favoriteService.RemoveFromFavoritesAsync(userId, workId);
        if (!success)
        {
            return this.BadRequest("Failed to remove from favorites.");
        }

        return this.Ok(new { message = "Removed from favorites." });
    }

    /// <summary>
    /// Перевіряє, чи є твір в обраному.
    /// </summary>
    /// <param name="workId">ID твору.</param>
    /// <returns>Boolean значення.</returns>
    [HttpGet("check/{workId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool))]
    public async Task<IActionResult> CheckFavorite(Guid workId)
    {
        var userId = this.GetUserId();
        if (userId == Guid.Empty)
        {
            return this.Ok(false);
        }

        var isFavorite = await this.favoriteService.IsFavoriteAsync(userId, workId);
        return this.Ok(isFavorite);
    }

    private Guid GetUserId()
    {
        var userIdClaim = this.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }
}
