namespace BookScreenExplorer.Infrastructure.Entities;

public class Author : BaseEntity
{
    public string FullName { get; set; } = null!;
    public DateOnly? BirthDate { get; set; }
    public string? Nationality { get; set; }
    public string? Biography { get; set; }

    public ICollection<BookAuthor> BookAuthors { get; set; } = new List<BookAuthor>();
}
