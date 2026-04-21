namespace Book2Screen.API__Web_.Controllers;

using System.Runtime.CompilerServices;
using AutoFilterer.Extensions;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Book2Screen.Application.DTOs;
using Book2Screen.Application.Filters;
using Book2Screen.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Контролер для роботи з каталогом елементів (книг/фільмів).
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ItemsController : ControllerBase
{
    private readonly ApplicationDbContext context;
    private readonly IMapper mapper;

    public ItemsController(ApplicationDbContext context, IMapper mapper)
    {
        this.context = context;
        this.mapper = mapper;
    }

    /// <summary>
    /// Отримати повний список усіх елементів.
    /// </summary>
    /// <returns>Список елементів.</returns>
    /// <response code="200">Список успішно отримано.</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllItems()
    {
        var list = await this.context.Adaptations.ProjectTo<AdaptationDto>(this.mapper.ConfigurationProvider).ToListAsync();
        return this.Ok(list);
    }

    /// <summary>
    /// Отримати деталі конкретного елемента за його ідентифікатором.
    /// </summary>
    /// <param name="id">Унікальний ідентифікатор елемента.</param>
    /// <returns>Детальна інформація про елемент.</returns>
    /// <response code="200">Елемент знайдено.</response>
    /// <response code="404">Елемент із таким ID не знайдено.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetItemById(int id)
    {
        var item = await this.context.Adaptations.FindAsync(id);
        if (item == null)
        {
            return this.NotFound();
        }

        return this.Ok(this.mapper.Map<AdaptationDto>(item));
    }

    /// <summary>
    /// Фільтрація каталогу за складними критеріями.
    /// </summary>
    /// <param name="filter">Параметри фільтрації (назва, тип, країна, діапазон років тощо).</param>
    /// <response code="200">Повертає відфільтрований список адаптацій.</response>
    [HttpGet("filter")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<AdaptationDto>))]
    public async Task<IActionResult> GetFilteredItems([FromQuery] AdaptationFilter filter)
    {
        var query = this.context.Adaptations.ApplyFilter(filter);

        var results = await query.ProjectTo<AdaptationDto>(this.mapper.ConfigurationProvider).ToListAsync();

        return this.Ok(results);
    }
}
