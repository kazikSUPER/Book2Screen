// <copyright file="WorksController.cs" company="Team 17">
// Copyright (c) Team 17. All rights reserved.
// </copyright>

namespace Book2Screen.API__Web_.Controllers;

using Book2Screen.Application.DTOs;
using Book2Screen.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Контролер для роботи з творами (Works) у форматі, який очікує фронтенд.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class WorksController : ControllerBase
{
    private readonly IAdaptationService adaptationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="WorksController"/> class.
    /// </summary>
    /// <param name="adaptationService">Сервіс адаптацій.</param>
    public WorksController(IAdaptationService adaptationService)
    {
        this.adaptationService = adaptationService;
    }

    /// <summary>
    /// Отримати список усіх творів (адаптацій) у форматі для головної сторінки.
    /// </summary>
    /// <returns>Список об'єктів BookScreenItem.</returns>
    /// <response code="200">Успішне отримання списку.</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<BookScreenItemDto>))]
    public async Task<IActionResult> GetWorks()
    {
        var adaptations = await this.adaptationService.GetAllAdaptationsAsync();

        var result = adaptations.Select(a => new BookScreenItemDto
        {
            Id = a.Id,
            Title = a.Title,
            Year = a.ReleaseYear ?? 0,
            Genre = "Драма",
            Country = a.Country ?? "Unknown",
            Poster = a.PosterUrl ?? "https://via.placeholder.com/300x450",
            BookRating = 4.5,
            FilmRating = 4.2,
            Description = a.Description ?? string.Empty,
        });

        return this.Ok(result);
    }

    /// <summary>
    /// Отримати деталі твору за його ідентифікатором.
    /// </summary>
    /// <param name="id">GUID твору.</param>
    /// <returns>Об'єкт BookScreenItem.</returns>
    /// <response code="200">Твір знайдено.</response>
    /// <response code="404">Твір з таким ID не знайдено.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BookScreenItemDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetWorkById(Guid id)
    {
        var a = await this.adaptationService.GetAdaptationByIdAsync(id);
        if (a == null)
        {
            return this.NotFound();
        }

        var result = new BookScreenItemDto
        {
            Id = a.Id,
            Title = a.Title,
            Year = a.ReleaseYear ?? 0,
            Genre = "Драма",
            Country = a.Country ?? "Unknown",
            Poster = a.PosterUrl ?? "https://via.placeholder.com/300x450",
            BookRating = 4.5,
            FilmRating = 4.2,
            Description = a.Description ?? string.Empty,
        };

        return this.Ok(result);
    }
}
