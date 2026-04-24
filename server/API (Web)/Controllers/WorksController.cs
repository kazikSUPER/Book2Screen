namespace Book2Screen.API__Web_.Controllers;

using Book2Screen.Application.DTOs;
using Book2Screen.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Контролер для роботи з творами (Works) у форматі, який очікує фронтенд.
/// </summary>
[ApiController]
[Route("api/v1/works")]
[Produces("application/json")]
public class WorksController : ControllerBase
{
    private readonly IAdaptationService adaptationService;

    public WorksController(IAdaptationService adaptationService)
    {
        this.adaptationService = adaptationService;
    }

    /// <summary>
    /// Отримати список усіх творів (адаптацій).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetWorks()
    {
        var adaptations = await this.adaptationService.GetAllAdaptationsAsync();
        
        // Мапимо AdaptationDto на формат BookScreenItem для фронтенду
        var result = adaptations.Select(a => new
        {
            id = a.Id,
            title = a.Title,
            year = a.ReleaseYear ?? 0,
            genre = "Драма", // Тимчасово
            country = a.Country ?? "Unknown",
            poster = a.PosterUrl ?? "https://via.placeholder.com/300x450",
            bookRating = 4.5, // Заглушка
            filmRating = 4.2, // Заглушка
            description = a.Description ?? ""
        });

        return this.Ok(result);
    }

    /// <summary>
    /// Отримати твір за ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetWorkById(Guid id)
    {
        var a = await this.adaptationService.GetAdaptationByIdAsync(id);
        if (a == null)
        {
            return this.NotFound();
        }

        var result = new
        {
            id = a.Id,
            title = a.Title,
            year = a.ReleaseYear ?? 0,
            genre = "Драма",
            country = a.Country ?? "Unknown",
            poster = a.PosterUrl ?? "https://via.placeholder.com/300x450",
            bookRating = 4.5,
            filmRating = 4.2,
            description = a.Description ?? ""
        };

        return this.Ok(result);
    }
}
