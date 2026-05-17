// <copyright file="ReviewsController.cs" company="Team 17">
// Copyright (c) Team 17. All rights reserved.
// </copyright>

namespace Book2Screen.API__Web_.Controllers;

using System.Security.Claims;
using Book2Screen.Application.DTOs;
using Book2Screen.Application.Interfaces;
using FluentValidation;
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
    private readonly IValidator<ReviewRequest> validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReviewsController"/> class.
    /// </summary>
    /// <param name="reviewService">Сервіс відгуків.</param>
    /// <param name="validator">Валідатор для запитів відгуків.</param>
    public ReviewsController(IReviewService reviewService, IValidator<ReviewRequest> validator)
    {
        this.reviewService = reviewService;
        this.validator = validator;
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
    /// <response code="400">Некоректні дані відгуку.</response>
    /// <response code="401">Користувач не авторизований.</response>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ReviewResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> AddReview([FromBody] ReviewRequest request)
    {
        var validationResult = await this.validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return this.BadRequest(validationResult.Errors);
        }

        var userIdClaim = this.User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
        {
            return this.Unauthorized();
        }

        var userId = Guid.Parse(userIdClaim.Value);
        var response = await this.reviewService.AddReviewAsync(userId, request);
        return this.Ok(response);
    }

    /// <summary>
    /// Оновити свій відгук (потрібна авторизація).
    /// </summary>
    /// <param name="id">ID відгуку.</param>
    /// <param name="request">Нові дані відгуку.</param>
    /// <response code="200">Відгук успішно оновлено.</response>
    /// <response code="400">Некоректні дані.</response>
    /// <response code="401">Не авторизований.</response>
    /// <response code="403">Це не ваш відгук або відгук не знайдено.</response>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [HttpPut("{id}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateReview(Guid id, [FromBody] ReviewRequest request)
    {
        var validationResult = await this.validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return this.BadRequest(validationResult.Errors);
        }

        var userIdClaim = this.User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
        {
            return this.Unauthorized();
        }

        var userId = Guid.Parse(userIdClaim.Value);
        var success = await this.reviewService.UpdateReviewAsync(userId, id, request);

        if (!success)
        {
            return this.Forbid();
        }

        return this.Ok(new { message = "Review updated successfully." });
    }

    /// <summary>
    /// Видалити свій відгук (потрібна авторизація).
    /// </summary>
    /// <param name="id">ID відгуку.</param>
    /// <response code="200">Відгук успішно видалено.</response>
    /// <response code="401">Не авторизований.</response>
    /// <response code="403">Це не ваш відгук або відгук не знайдено.</response>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [HttpDelete("{id}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteReview(Guid id)
    {
        var userIdClaim = this.User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
        {
            return this.Unauthorized();
        }

        var userId = Guid.Parse(userIdClaim.Value);
        var success = await this.reviewService.DeleteReviewAsync(userId, id);

        if (!success)
        {
            return this.Forbid();
        }

        return this.Ok(new { message = "Review deleted successfully." });
    }
}
