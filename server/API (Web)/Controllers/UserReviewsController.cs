// <copyright file="UserReviewsController.cs" company="Team 17">
// Copyright (c) Team 17. All rights reserved.
// </copyright>

namespace Book2Screen.API__Web_.Controllers;

using System.Security.Claims;
using Book2Screen.Application.DTOs;
using Book2Screen.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Контролер для керування відгуками поточного користувача.
/// </summary>
[ApiController]
[Route("api/v1/users/me/reviews")]
[Authorize]
[Produces("application/json")]
public class UserReviewsController : ControllerBase
{
    private readonly IReviewService reviewService;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserReviewsController"/> class.
    /// </summary>
    /// <param name="reviewService">Сервіс відгуків.</param>
    public UserReviewsController(IReviewService reviewService)
    {
        this.reviewService = reviewService;
    }

    /// <summary>
    /// Отримати список відгуків поточного користувача.
    /// </summary>
    /// <response code="200">Повертає список відгуків.</response>
    /// <response code="401">Користувач не авторизований.</response>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ReviewResponse>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMyReviews()
    {
        var userIdClaim = this.User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
        {
            return this.Unauthorized();
        }

        var userId = Guid.Parse(userIdClaim.Value);
        var reviews = await this.reviewService.GetUserReviewsAsync(userId);
        return this.Ok(reviews);
    }
}
