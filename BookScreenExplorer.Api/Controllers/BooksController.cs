using BookScreenExplorer.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookScreenExplorer.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public BooksController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetBooks()
    {
        var books = await _context.Books
            .AsNoTracking()
            .OrderBy(b => b.Title)
            .Select(b => new
            {
                b.Id,
                b.Title,
                b.Description,
                b.Genre,
                b.PublicationYear,
                b.Language,
                b.CoverImageUrl,
                b.CreatedAt,
                b.UpdatedAt
            })
            .ToListAsync();

        return Ok(books);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetBookById(Guid id)
    {
        var book = await _context.Books
            .AsNoTracking()
            .Where(b => b.Id == id)
            .Select(b => new
            {
                b.Id,
                b.Title,
                b.Description,
                b.Genre,
                b.PublicationYear,
                b.Language,
                b.CoverImageUrl,
                b.CreatedAt,
                b.UpdatedAt
            })
            .FirstOrDefaultAsync();

        if (book is null)
        {
            return NotFound(new
            {
                message = "Book not found",
                requestedId = id
            });
        }

        return Ok(book);
    }
}