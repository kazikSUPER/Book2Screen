// <copyright file="AdminReportsController.cs" company="Team 17">
// Copyright (c) Team 17. All rights reserved.
// </copyright>

namespace Book2Screen.API__Web_.Controllers;

using Book2Screen.Application.DTOs;
using Book2Screen.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Контролер для керування скаргами (адмін-панель).
/// </summary>
[ApiController]
[Route("api/v1/admin/reports")]
[Authorize(Roles = "admin")]
[Produces("application/json")]
public class AdminReportsController : ControllerBase
{
    private readonly IReviewService reviewService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminReportsController"/> class.
    /// </summary>
    /// <param name="reviewService">Сервіс відгуків.</param>
    public AdminReportsController(IReviewService reviewService)
    {
        this.reviewService = reviewService;
    }

    /// <summary>
    /// Отримати список усіх скарг.
    /// </summary>
    /// <returns>Список скарг.</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ReportResponse>))]
    public async Task<IActionResult> GetAllReports()
    {
        var reports = await this.reviewService.GetAllReportsAsync();
        return this.Ok(reports);
    }

    /// <summary>
    /// Схвалити скаргу (видалити відгук).
    /// </summary>
    /// <param name="id">ID скарги.</param>
    /// <returns>Ok.</returns>
    [HttpPost("{id:guid}/approve")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ApproveReport(Guid id)
    {
        await this.reviewService.ModerateReviewAsync(id, "approve");
        return this.Ok(new { message = "Report approved, review removed." });
    }

    /// <summary>
    /// Відхилити скаргу.
    /// </summary>
    /// <param name="id">ID скарги.</param>
    /// <returns>Ok.</returns>
    [HttpPost("{id:guid}/reject")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> RejectReport(Guid id)
    {
        await this.reviewService.ModerateReviewAsync(id, "reject");
        return this.Ok(new { message = "Report rejected." });
    }

    /// <summary>
    /// Позначити відгук як спойлер.
    /// </summary>
    /// <param name="id">ID скарги.</param>
    /// <returns>Ok.</returns>
    [HttpPost("{id:guid}/spoiler")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> MarkAsSpoiler(Guid id)
    {
        await this.reviewService.ModerateReviewAsync(id, "spoiler");
        return this.Ok(new { message = "Review marked as spoiler." });
    }
}
