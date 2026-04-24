namespace Book2Screen.API__Web_.Controllers;

using System.Security.Claims;
using Book2Screen.Application.DTOs;
using Book2Screen.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Контролер для керування голосуваннями "Книга проти Фільму".
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class VotesController : ControllerBase
{
    private readonly IVoteService voteService;

    public VotesController(IVoteService voteService)
    {
        this.voteService = voteService;
    }

    /// <summary>
    /// Отримати актуальну статистику голосів для твору.
    /// </summary>
    /// <param name="workId">ID твору.</param>
    /// <response code="200">Повертає об'єкт зі статистикою голосування.</response>
    [HttpGet("{workId}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(VoteResponse))]
    public async Task<IActionResult> GetVoteStats(Guid workId)
    {
        var stats = await this.voteService.GetVoteStatsAsync(workId);
        return this.Ok(stats);
    }

    /// <summary>
    /// Проголосувати за твір (потрібна авторизація).
    /// </summary>
    /// <remarks>
    /// Користувач може проголосувати лише один раз за один твір. Повторний запит оновить попередній голос.
    /// </remarks>
    /// <param name="request">Дані голосування (ID твору та тип "book" або "movie").</param>
    /// <response code="200">Голос успішно враховано, повертає оновлену статистику.</response>
    /// <response code="401">Користувач не авторизований.</response>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(VoteResponse))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Vote([FromBody] VoteRequest request)
    {
        var userIdClaim = this.User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
        {
            return this.Unauthorized();
        }

        var userId = Guid.Parse(userIdClaim.Value);
        var stats = await this.voteService.VoteAsync(userId, request);
        return this.Ok(stats);
    }
}
