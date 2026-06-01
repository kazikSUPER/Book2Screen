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
    [System.Diagnostics.CodeAnalysis.SuppressMessage("SonarLint", "S1075:URIs should not be hardcoded", Justification = "Seed data requires hardcoded URLs for development avatars and posters.")]
    private const string DunePosterUrl = "https://m.media-amazon.com/images/M/MV5BMDQ0NjgyN2YtNWU4Ny00YjZlLWIzMTktMzljZWM2MTgzY2RhXkEyXkFqcGdeQXVyNjU0OTQ0OTY@._V1_.jpg";

    [System.Diagnostics.CodeAnalysis.SuppressMessage("SonarLint", "S1075:URIs should not be hardcoded", Justification = "Seed data requires hardcoded URLs for development avatars.")]
    private const string AdminAvatarUrl = "https://ui-avatars.com/api/?name=Admin&background=random";

    [System.Diagnostics.CodeAnalysis.SuppressMessage("SonarLint", "S1075:URIs should not be hardcoded", Justification = "Seed data requires hardcoded URLs for development avatars.")]
    private const string UserAvatarUrl = "https://ui-avatars.com/api/?name=John+Doe&background=random";

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
                AvatarUrl = AdminAvatarUrl,
                IsActive = true,
            };

            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = "john_doe",
                Email = "john@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("User123!"),
                Role = "user",
                AvatarUrl = UserAvatarUrl,
                IsActive = true,
            };

            await context.Users.AddRangeAsync(admin, user);
            await context.SaveChangesAsync();
        }

        // 2. Перевірка контенту
        if (!await context.Books.AnyAsync(b => b.Title == "Дюна"))
        {
            var author = new Author
            {
                Id = Guid.NewGuid(),
                FullName = "Френк Герберт",
                Nationality = "Американець",
                Biography = "Американський письменник-фантаст, найбільш відомий як автор науково-фантастичного роману «Дюна».",
            };

            var actor = new Actor
            {
                Id = Guid.NewGuid(),
                FullName = "Тімоті Шаламе",
                Nationality = "Американець/Француз",
                Biography = "Актор, номінований на премію «Оскар».",
            };

            var book = new Book
            {
                Id = Guid.NewGuid(),
                Title = "Дюна",
                Description = "Історія про подорож юнака до пустельної планети Арракіс.",
                Genre = "Наукова фантастика",
                PublicationYear = 1965,
                Language = "Українська",
                Authors = new List<Author> { author },
            };

            var adaptation = new Adaptation
            {
                Id = Guid.NewGuid(),
                Title = "Дюна: Частина перша",
                Type = "movie",
                Description = "Епічний науково-фантастичний фільм Дені Вільньова 2021 року.",
                ReleaseYear = 2021,
                DurationMinutes = 155,
                Studio = "Legendary Pictures",
                Country = "США",
                PosterUrl = DunePosterUrl,
            };

            var work = new Work
            {
                Id = Guid.NewGuid(),
                Book = book,
                Adaptation = adaptation,
                Title = "Дюна: Книга проти фільму 2021 року",
                Summary = "Порівняння шедевра Френка Герберта та екранізації Вільньова.",
            };

            var adaptationActor = new AdaptationActor
            {
                Adaptation = adaptation,
                Actor = actor,
                RoleName = "Пол Атрід",
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
                    Text = "Фільм візуально приголомшливий, але книга пропонує набагато глибшу проробку світу. Спойлер: Пол виживає!",
                    IsSpoiler = false, // Початковий стан, буде позначено скаргою
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
                        Reason = "Містить приховані спойлери без відповідного тегу",
                        Status = "Pending",
                    };
                    await context.Reports.AddAsync(report);
                }
            }
        }

        await context.SaveChangesAsync();
    }
}
