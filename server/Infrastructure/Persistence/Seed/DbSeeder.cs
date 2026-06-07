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
    private const string DunePosterUrl = "https://upload.wikimedia.org/wikipedia/uk/7/71/%D0%94%D1%8E%D0%BD%D0%B0_%282021%29_%D0%BF%D0%BE%D1%81%D1%82%D0%B5%D1%80.jpg";

    [System.Diagnostics.CodeAnalysis.SuppressMessage("SonarLint", "S1075:URIs should not be hardcoded", Justification = "Seed data requires hardcoded URLs for development avatars and posters.")]
    private const string LotrPosterUrl = "https://upload.wikimedia.org/wikipedia/uk/8/8a/%D0%92%D0%BE%D0%BB%D0%BE%D0%B4%D0%B0%D1%80_%D0%BF%D0%B5%D1%80%D1%81%D0%BD%D1%96%D0%B2_%D0%A5%D1%80%D0%B0%D0%BD%D0%B8%D1%82%D0%B5%D0%BB%D1%96_%D0%BA%D1%96%D0%BB%D1%8C%D1%86%D1%8F_%D0%BF%D0%BE%D1%81%D1%82%D0%B5%D1%80.jpg";

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
        User admin;
        User user;
        if (!await context.Users.AnyAsync(u => u.Email == "admin@book2screen.com"))
        {
            admin = new User
            {
                Id = Guid.NewGuid(),
                Username = "admin",
                Email = "admin@book2screen.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                Role = "admin",
                AvatarUrl = AdminAvatarUrl,
                IsActive = true,
            };

            user = new User
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
        else
        {
            admin = await context.Users.FirstAsync(u => u.Username == "admin");
            user = await context.Users.FirstAsync(u => u.Username == "john_doe");
        }

        // 2. Перевірка контенту (Дюна)
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

            var map = new DifferenceMap
            {
                Id = Guid.NewGuid(),
                WorkId = work.Id,
                Title = "Мапа розбіжностей Дюни",
                Differences = new List<Difference>
                {
                    new Difference { Id = Guid.NewGuid(), Title = "Зміна сюжету", BookText = "У книзі сцена вечері з банкірами детально описана.", FilmText = "У фільмі відсутня сцена вечері з банкірами.", ImportanceLevel = "medium" },
                    new Difference { Id = Guid.NewGuid(), Title = "Персонаж", BookText = "Доктор Лайт-Кіндс у книзі — чоловік.", FilmText = "Доктор Лайт-Кіндс у фільмі — жінка.", ImportanceLevel = "high", IsSpoiler = true },
                },
            };

            await context.Works.AddAsync(work);
            await context.Set<AdaptationActor>().AddAsync(adaptationActor);
            await context.Ratings.AddAsync(rating);
            await context.DifferenceMaps.AddAsync(map);

            var review = new Review
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                WorkId = work.Id,
                TargetType = "comparison",
                Text = "Фільм візуально приголомшливий, але книга пропонує набагато глибшу проробку світу. Спойлер: Пол виживає!",
                IsSpoiler = false,
                Rating = 9.0,
                LikesCount = 10,
            };
            await context.Reviews.AddAsync(review);

            var report = new Report
            {
                Id = Guid.NewGuid(),
                UserId = admin.Id,
                ReviewId = review.Id,
                Reason = "Містить приховані спойлери без відповідного тегу",
                Status = "Pending",
            };
            await context.Reports.AddAsync(report);
        }

        // 3. Додатковий твір (Володар Перснів)
        if (!await context.Books.AnyAsync(b => b.Title == "Братство Персня"))
        {
            var author = new Author { Id = Guid.NewGuid(), FullName = "Дж. Р. Р. Толкін", Nationality = "Британець" };
            var actor = new Actor { Id = Guid.NewGuid(), FullName = "Елайджа Вуд", Nationality = "Американець" };

            var book = new Book
            {
                Id = Guid.NewGuid(),
                Title = "Братство Персня",
                Genre = "Фентезі",
                PublicationYear = 1954,
                Authors = new List<Author> { author },
            };

            var adaptation = new Adaptation
            {
                Id = Guid.NewGuid(),
                Title = "Володар перснів: Хранителі персня",
                Type = "movie",
                ReleaseYear = 2001,
                PosterUrl = LotrPosterUrl,
            };

            var work = new Work
            {
                Id = Guid.NewGuid(),
                Book = book,
                Adaptation = adaptation,
                Title = "Володар Перснів (2001)",
                Summary = "Культова трилогія Пітера Джексона.",
            };

            var adaptationActor = new AdaptationActor { Adaptation = adaptation, Actor = actor, RoleName = "Фродо Беггінс", };

            await context.Works.AddAsync(work);
            await context.Set<AdaptationActor>().AddAsync(adaptationActor);

            // Додаємо в обране для користувача
            await context.Favorites.AddAsync(new Favorite { UserId = user.Id, WorkId = work.Id, Kind = "read", });
        }

        await context.SaveChangesAsync();
    }
}
