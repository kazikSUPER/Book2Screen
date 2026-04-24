namespace Book2Screen.API__Web_.Controllers;

using Book2Screen.Application.DTOs;
using Book2Screen.Application.Filters;
using Book2Screen.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Контролер для роботи з каталогом елементів (книг/фільмів).
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ItemsController : ControllerBase
{
    private readonly IAdaptationService adaptationService;

    public ItemsController(IAdaptationService adaptationService)
    {
        this.adaptationService = adaptationService;
    }

    /// <summary>
    /// Отримати повний список усіх елементів.
    /// </summary>
    /// <returns>Список елементів.</returns>
    /// <response code="200">Список успішно отримано.</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<AdaptationDto>))]
    public async Task<IActionResult> GetAllItems()
    {
        var list = await this.adaptationService.GetAllAdaptationsAsync();
        return this.Ok(list);
    }

    /// <summary>
    /// Отримати деталі конкретного елемента за його ідентифікатором.
    /// </summary>
    /// <param name="id">Унікальний ідентифікатор елемента (GUID).</param>
    /// <returns>Детальна інформація про елемент.</returns>
    /// <response code="200">Елемент знайдено.</response>
    /// <response code="404">Елемент із таким ID не знайдено.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AdaptationDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetItemById(Guid id)
    {
        var item = await this.adaptationService.GetAdaptationByIdAsync(id);
        if (item == null)
        {
            return this.NotFound();
        }

        return this.Ok(item);
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
        var results = await this.adaptationService.GetFilteredAdaptationsAsync(filter);
        return this.Ok(results);
    }
}
