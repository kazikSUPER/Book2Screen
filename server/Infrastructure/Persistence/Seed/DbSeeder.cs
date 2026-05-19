// <copyright file="DbSeeder.cs" company="Team 17">
// Copyright (c) Team 17. All rights reserved.
// </copyright>

namespace Book2Screen.Infrastructure.Persistence.Seed;

using Book2Screen.Domain.Entities;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Клас для наповнення бази даних початковими даними (Seed data).
/// </summary>
public static class DbSeeder
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("SonarLint", "S1075:URIs should not be hardcoded", Justification = "Seed data requires hardcoded URLs for development posters.")]
    private static readonly string DunePosterUrl = "https://upload.wikimedia.org/wikipedia/uk/7/71/%D0%94%D1%8E%D0%BD%D0%B0_%282021%29_%D0%BF%D0%BE%D1%81%D1%82%D0%B5%D1%80.jpg";

    /// <summary>
    /// Наповнює базу даних, якщо вона порожня.
    /// </summary>
    /// <param name="context">Контекст бази даних.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        // 1. Перевірка користувачів
        if (!await context.Users.AnyAsync(u => u.Email == "admin@book2screen.com"))
        {
            var admin = new User
            {
                Id = Guid.NewGuid(),
                Username = "admin",
                Email = "admin@book2screen.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                Role = "admin",
                AvatarUrl = "https://ui-avatars.com/api/?name=Admin&background=random",
                IsActive = true,
            };

            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = "john_doe",
                Email = "john@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("User123!"),
                Role = "user",
                AvatarUrl = "https://ui-avatars.com/api/?name=John+Doe&background=random",
                IsActive = true,
            };

            await context.Users.AddRangeAsync(admin, user);
            await context.SaveChangesAsync();
        }

        // 2. Перевірка контенту
        if (!await context.Books.AnyAsync(b => b.Title == "Dune"))
        {
            // ... (author, actor, book, adaptation creation logic remains same)
            var author = new Author
            {
                Id = Guid.NewGuid(),
                FullName = "Frank Herbert",
                Nationality = "American",
                Biography = "American science fiction novelist best known for the novel Dune.",
            };

            var actor = new Actor
            {
                Id = Guid.NewGuid(),
                FullName = "Timothée Chalamet",
                Nationality = "American/French",
                Biography = "Academy Award-nominated actor.",
            };

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
                PosterUrl = DunePosterUrl,
            };

            var work = new Work
            {
                Id = Guid.NewGuid(),
                Book = book,
                Adaptation = adaptation,
                Title = "Dune: Book vs 2021 Movie",
                Summary = "A comparison between Frank Herbert's masterpiece and Villeneuve's adaptation.",
            };

            var adaptationActor = new AdaptationActor
            {
                Adaptation = adaptation,
                Actor = actor,
                RoleName = "Paul Atreides",
            };

            var rating = new Rating
            {
                Id = Guid.NewGuid(),
                Work = work,
                BookRating = 9.5m,
                AdaptationRating = 8.9m,
                VotesCount = 1,
            };

            await context.Works.AddAsync(work);
            await context.Set<AdaptationActor>().AddAsync(adaptationActor);
            await context.Ratings.AddAsync(rating);

            var johnUser = await context.Users.FirstOrDefaultAsync(u => u.Username == "john_doe");
            var adminUser = await context.Users.FirstOrDefaultAsync(u => u.Username == "admin");

            if (johnUser != null)
            {
                var review = new Review
                {
                    Id = Guid.NewGuid(),
                    UserId = johnUser.Id,
                    WorkId = work.Id,
                    TargetType = "comparison",
                    Text = "The movie is visually stunning, but the book offers much more world-building. Spoiler: Paul survives!",
                    IsSpoiler = false, // Initial state, will be reported
                    LikesCount = 10,
                };
                await context.Reviews.AddAsync(review);

                // Додаємо скаргу на цей відгук
                if (adminUser != null)
                {
                    var report = new Report
                    {
                        Id = Guid.NewGuid(),
                        UserId = adminUser.Id,
                        ReviewId = review.Id,
                        Reason = "Contains hidden spoilers without tag",
                        Status = "Pending",
                    };
                    await context.Reports.AddAsync(report);
                }
            }
        }

        await context.SaveChangesAsync();
    }
}
