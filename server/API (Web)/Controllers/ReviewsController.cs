// <copyright file="ReviewsController.cs" company="Team 17">
// Copyright (c) Team 17. All rights reserved.
// </copyright>

namespace Book2Screen.API__Web_.Controllers;

using System.Security.Claims;
using Book2Screen.Application.DTOs;
using Book2Screen.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Контролер для керування відгуками користувачів.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class ReviewsController : ControllerBase
{
    private readonly IReviewService reviewService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReviewsController"/> class.
    /// </summary>
    /// <param name="reviewService">Сервіс відгуків.</param>
    public ReviewsController(IReviewService reviewService)
    {
        this.reviewService = reviewService;
    }

    /// <summary>
    /// Отримати всі відгуки для конкретного твору.
    /// </summary>
    /// <param name="workId">ID твору.</param>
    /// <response code="200">Повертає список відгуків.</response>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [HttpGet("work/{workId}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ReviewResponse>))]
    public async Task<IActionResult> GetReviewsByWork(Guid workId)
    {
        var reviews = await this.reviewService.GetReviewsByWorkIdAsync(workId);
        return this.Ok(reviews);
    }

    /// <summary>
    /// Додати новий відгук (потрібна авторизація).
    /// </summary>
    /// <param name="request">Дані відгуку (текст, рейтинг, мітка спойлера).</param>
    /// <response code="200">Відгук успішно додано.</response>
    /// <response code="401">Користувач не авторизований.</response>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ReviewResponse))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> AddReview([FromBody] ReviewRequest request)
    {
        var userIdClaim = this.User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
        {
            return this.Unauthorized();
        }

        var userId = Guid.Parse(userIdClaim.Value);
        var response = await this.reviewService.AddReviewAsync(userId, request);
        return this.Ok(response);
    }
}
