namespace Book2Screen.Infrastructure.Persistence.Seed;

using Book2Screen.Domain.Entities;
using Microsoft.EntityFrameworkCore;

public static class DbSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
{
    // 1. Перевірка користувачів (за Email, бо він Unique)
    if (!await context.Users.AnyAsync(u => u.Email == "admin@book2screen.com"))
    {
        var admin = new User
        {
            Id = Guid.NewGuid(),
            Username = "admin",
            Email = "admin@book2screen.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
            Role = "admin",
            IsActive = true,
        };

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "john_doe",
            Email = "john@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("User123!"),
            Role = "user",
            IsActive = true,
        };

        await context.Users.AddRangeAsync(admin, user);
    }

    // 2. Перевірка контенту (за назвою книги, щоб уникнути дублів у 1:1 Work)
    if (!await context.Books.AnyAsync(b => b.Title == "Dune"))
    {
        // Створюємо автора
        var author = new Author
        {
            Id = Guid.NewGuid(),
            FullName = "Frank Herbert",
            Nationality = "American",
            Biography = "American science fiction novelist best known for the novel Dune.",
        };

        // Створюємо актора
        var actor = new Actor
        {
            Id = Guid.NewGuid(),
            FullName = "Timothée Chalamet",
            Nationality = "American/French",
            Biography = "Academy Award-nominated actor.",
        };

        // Створюємо книгу
        var book = new Book
        {
            Id = Guid.NewGuid(),
            Title = "Dune",
            Description = "A story about a young man's journey to the desert planet Arrakis.",
            Genre = "Sci-Fi",
            PublicationYear = 1965,
            Language = "English",
            Authors = new List<Author> { author },
        };

        // Створюємо адаптацію
        var adaptation = new Adaptation
        {
            Id = Guid.NewGuid(),
            Title = "Dune: Part One",
            Type = "movie",
            Description = "Denis Villeneuve's 2021 epic science fiction film.",
            ReleaseYear = 2021,
            DurationMinutes = 155,
            Studio = "Legendary Pictures",
            Country = "USA",
            PosterUrl = "https://upload.wikimedia.org/wikipedia/uk/7/71/%D0%94%D1%8E%D0%BD%D0%B0_%282021%29_%D0%BF%D0%BE%D1%81%D1%82%D0%B5%D1%80.jpg",
        };

        // Створюємо Work (головний об'єкт порівняння)
        var work = new Work
        {
            Id = Guid.NewGuid(),
            Book = book,
            Adaptation = adaptation,
            Title = "Dune: Book vs 2021 Movie",
            Summary = "A comparison between Frank Herbert's masterpiece and Villeneuve's adaptation.",
        };

        // Створюємо зв'язок Актор-Адаптація через проміжну сутність
        var adaptationActor = new AdaptationActor
        {
            Adaptation = adaptation,
            Actor = actor,
            RoleName = "Paul Atreides",
        };

        // Створюємо рейтинг
        var rating = new Rating
        {
            Id = Guid.NewGuid(),
            Work = work,
            BookRating = 9.5m,
            AdaptationRating = 8.9m,
            VotesCount = 1,
        };

        // Додаємо все в контекст
        // EF Core автоматично підтягне залежні об'єкти (author, actor),
        // якщо вони додані до властивостей основних об'єктів
        await context.Works.AddAsync(work);
        await context.Set<AdaptationActor>().AddAsync(adaptationActor);
        await context.Ratings.AddAsync(rating);

        // Опційно: додаємо відгук, якщо користувач вже існує або був створений вище
        var existingUser = await context.Users.FirstOrDefaultAsync(u => u.Username == "john_doe");
        if (existingUser != null)
        {
            var review = new Review
            {
                Id = Guid.NewGuid(),
                User = existingUser,
                Work = work,
                TargetType = "comparison",
                Text = "The movie is visually stunning, but the book offers much more world-building.",
                LikesCount = 10,
            };
            await context.Reviews.AddAsync(review);
        }
    }

    // Один загальний SaveChanges для всієї транзакції сідінгу
    await context.SaveChangesAsync();
}
}
