// <copyright file="AdminController.cs" company="Team 17">
// Copyright (c) Team 17. All rights reserved.
// </copyright>

namespace Book2Screen.API__Web_.Controllers;

using Book2Screen.Application.DTOs;
using Book2Screen.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Контролер для адміністративних дій.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Roles = "admin")]
[Produces("application/json")]
public class AdminController : ControllerBase
{
    private readonly IAdaptationService adaptationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminController"/> class.
    /// </summary>
    /// <param name="adaptationService">Сервіс адаптацій.</param>
    public AdminController(IAdaptationService adaptationService)
    {
        this.adaptationService = adaptationService;
    }

    /// <summary>
    /// Створити нову адаптацію.
    /// </summary>
    /// <param name="adaptationDto">Дані адаптації.</param>
    /// <returns>Створена адаптація.</returns>
    [HttpPost("adaptations")]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(AdaptationDto))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateAdaptation([FromBody] AdaptationDto adaptationDto)
    {
        var result = await this.adaptationService.CreateAdaptationAsync(adaptationDto);
        return this.CreatedAtAction(nameof(this.GetAdaptationById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Отримати адаптацію за ID (для адмін-панелі).
    /// </summary>
    /// <param name="id">ID адаптації.</param>
    /// <returns>Адаптація.</returns>
    [HttpGet("adaptations/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AdaptationDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAdaptationById(Guid id)
    {
        var result = await this.adaptationService.GetAdaptationByIdAsync(id);
        if (result == null)
        {
            return this.NotFound();
        }

        return this.Ok(result);
    }

    /// <summary>
    /// Оновити адаптацію.
    /// </summary>
    /// <param name="id">ID адаптації.</param>
    /// <param name="adaptationDto">Нові дані.</param>
    /// <returns>Оновлена адаптація.</returns>
    [HttpPut("adaptations/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AdaptationDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAdaptation(Guid id, [FromBody] AdaptationDto adaptationDto)
    {
        var result = await this.adaptationService.UpdateAdaptationAsync(id, adaptationDto);
        if (result == null)
        {
            return this.NotFound();
        }

        return this.Ok(result);
    }

    /// <summary>
    /// Видалити адаптацію.
    /// </summary>
    /// <param name="id">ID адаптації.</param>
    /// <returns>NoContent.</returns>
    [HttpDelete("adaptations/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAdaptation(Guid id)
    {
        var success = await this.adaptationService.DeleteAdaptationAsync(id);
        if (!success)
        {
            return this.NotFound();
        }

        return this.NoContent();
    }
}
