using BookScreenExplorer.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookScreenExplorer.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WorksController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public WorksController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetWorks()
    {
        var works = await _context.Works
            .AsNoTracking()
            .OrderBy(w => w.Title)
            .Select(w => new
            {
                w.Id,
                w.Title,
                w.Summary,
                w.BookId,
                w.AdaptationId
            })
            .ToListAsync();

        return Ok(works);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetWorkById(Guid id)
    {
        var work = await _context.Works
            .AsNoTracking()
            .Where(w => w.Id == id)
            .Select(w => new
            {
                w.Id,
                w.Title,
                w.Summary,
                w.BookId,
                w.AdaptationId
            })
            .FirstOrDefaultAsync();

        if (work is null)
        {
            return NotFound(new
            {
                message = "Work not found",
                requestedId = id
            });
        }

        return Ok(work);
    }
}