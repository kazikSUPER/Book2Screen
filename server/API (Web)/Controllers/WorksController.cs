// <copyright file="WorksController.cs" company="Team 17">
// Copyright (c) Team 17. All rights reserved.
// </copyright>

namespace Book2Screen.API__Web_.Controllers;

using Book2Screen.Application.DTOs;
using Book2Screen.Application.Filters;
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
    private readonly IWorkService workService;

    /// <summary>
    /// Initializes a new instance of the <see cref="WorksController"/> class.
    /// </summary>
    /// <param name="workService">Сервіс творів.</param>
    public WorksController(IWorkService workService)
    {
        this.workService = workService;
    }

    /// <summary>
    /// Отримати список усіх творів у форматі для головної сторінки.
    /// </summary>
    /// <param name="filter">Фільтри (пошук, жанр, країна).</param>
    /// <returns>Список об'єктів BookScreenItemDto.</returns>
    /// <response code="200">Успішне отримання списку.</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<BookScreenItemDto>))]
    public async Task<IActionResult> GetWorks([FromQuery] WorkFilter filter)
    {
        var result = await this.workService.GetAllWorksAsync(filter);
        return this.Ok(result);
    }

    /// <summary>
    /// Отримати топ творів за рейтингом адаптації.
    /// </summary>
    /// <param name="count">Кількість творів (за замовчуванням 10).</param>
    /// <returns>Список об'єктів BookScreenItemDto.</returns>
    /// <response code="200">Успішне отримання списку.</response>
    [HttpGet("top")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<BookScreenItemDto>))]
    public async Task<IActionResult> GetTopWorks([FromQuery] int count = 10)
    {
        var result = await this.workService.GetTopWorksAsync(count);
        return this.Ok(result);
    }

    /// <summary>
    /// Отримати деталі твору за його ідентифікатором.
    /// </summary>
    /// <param name="id">GUID твору.</param>
    /// <returns>Об'єкт BookScreenItemDto.</returns>
    /// <response code="200">Твір знайдено.</response>
    /// <response code="404">Твір з таким ID не знайдено.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BookScreenItemDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetWorkById(Guid id)
    {
        var result = await this.workService.GetWorkByIdAsync(id);
        if (result == null)
        {
            return this.NotFound();
        }

        return this.Ok(result);
    }
}
