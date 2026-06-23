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
    private const string DunePosterUrl = "https://upload.wikimedia.org/wikipedia/uk/7/71/Дюна_%282021%29_постер.jpg";

    [System.Diagnostics.CodeAnalysis.SuppressMessage("SonarLint", "S1075:URIs should not be hardcoded", Justification = "Seed data requires hardcoded URLs for development avatars and posters.")]
    private const string LotrPosterUrl = "https://upload.wikimedia.org/wikipedia/uk/0/0c/The_Fellowship_Of_The_Ring.jpg";

    [System.Diagnostics.CodeAnalysis.SuppressMessage("SonarLint", "S1075:URIs should not be hardcoded", Justification = "Seed data requires hardcoded URLs for development avatars and posters.")]
    private const string HarryPotterPosterUrl = "https://upload.wikimedia.org/wikipedia/uk/c/c5/ГПФКПостер.jpg";

    [System.Diagnostics.CodeAnalysis.SuppressMessage("SonarLint", "S1075:URIs should not be hardcoded", Justification = "Seed data requires hardcoded URLs for development avatars.")]
    private const string DuneBookCoverUrl = "https://upload.wikimedia.org/wikipedia/uk/9/9a/Duna_UKR_2017_KSD_palityrka.jpg";

    [System.Diagnostics.CodeAnalysis.SuppressMessage("SonarLint", "S1075:URIs should not be hardcoded", Justification = "Seed data requires hardcoded URLs for development avatars.")]
    private const string LotrBookCoverUrl = "https://upload.wikimedia.org/wikipedia/uk/8/8c/Братство_Персня.jpg";

    [System.Diagnostics.CodeAnalysis.SuppressMessage("SonarLint", "S1075:URIs should not be hardcoded", Justification = "Seed data requires hardcoded URLs for development avatars.")]
    private const string HpBookCoverUrl = "https://upload.wikimedia.org/wikipedia/uk/6/6c/HPandPhStone_Ukr.jpg";

    [System.Diagnostics.CodeAnalysis.SuppressMessage("SonarLint", "S1075:URIs should not be hardcoded", Justification = "Seed data requires hardcoded URLs for development avatars.")]
    private const string FightClubPosterUrl = "https://covers.openlibrary.org/b/isbn/9780393327342-L.jpg";

    [System.Diagnostics.CodeAnalysis.SuppressMessage("SonarLint", "S1075:URIs should not be hardcoded", Justification = "Seed data requires hardcoded URLs for development avatars.")]
    private const string WitcherPosterUrl = "https://covers.openlibrary.org/b/isbn/9780316438964-L.jpg";

    [System.Diagnostics.CodeAnalysis.SuppressMessage("SonarLint", "S1075:URIs should not be hardcoded", Justification = "Seed data requires hardcoded URLs for development avatars.")]
    private const string FightClubBookCoverUrl = "https://covers.openlibrary.org/b/isbn/9780393355949-L.jpg";

    [System.Diagnostics.CodeAnalysis.SuppressMessage("SonarLint", "S1075:URIs should not be hardcoded", Justification = "Seed data requires hardcoded URLs for development avatars.")]
    private const string WitcherBookCoverUrl = "https://covers.openlibrary.org/b/isbn/9780316333528-L.jpg";

    // --- Golden Data (Stage 7) ---
    [System.Diagnostics.CodeAnalysis.SuppressMessage("SonarLint", "S1075:URIs should not be hardcoded", Justification = "Seed data requires hardcoded URLs.")]
    private const string KaidashBookCoverUrl = "https://static.yakaboo.ua/media/cloudflare/product/webp/600x840/1/6/16_2_96.jpg";
    [System.Diagnostics.CodeAnalysis.SuppressMessage("SonarLint", "S1075:URIs should not be hardcoded", Justification = "Seed data requires hardcoded URLs.")]
    private const string KaidashPosterUrl = "https://kino-teatr.ua/public/main/films/2020-03/trailer_18524.jpg";

    [System.Diagnostics.CodeAnalysis.SuppressMessage("SonarLint", "S1075:URIs should not be hardcoded", Justification = "Seed data requires hardcoded URLs.")]
    private const string BerkutBookCoverUrl = "https://upload.wikimedia.org/wikipedia/uk/1/1f/Zahar_Berkut_%28UKR_paliturka%2C_1986%2C_Kameniar%29.jpeg";
    [System.Diagnostics.CodeAnalysis.SuppressMessage("SonarLint", "S1075:URIs should not be hardcoded", Justification = "Seed data requires hardcoded URLs.")]
    private const string BerkutPosterUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/b/bb/The_Rising_Hawk_poster_UA.jpg/960px-The_Rising_Hawk_poster_UA.jpg";

    [System.Diagnostics.CodeAnalysis.SuppressMessage("SonarLint", "S1075:URIs should not be hardcoded", Justification = "Seed data requires hardcoded URLs.")]
    private const string GreenMileBookCoverUrl = "https://upload.wikimedia.org/wikipedia/uk/4/48/The_Green_Mile._Stephen_King.jpg";
    [System.Diagnostics.CodeAnalysis.SuppressMessage("SonarLint", "S1075:URIs should not be hardcoded", Justification = "Seed data requires hardcoded URLs.")]
    private const string GreenMilePosterUrl = "https://m.media-amazon.com/images/M/MV5BMTUxMzQyNjA5MF5BMl5BanBnXkFtZTYwOTU2NTY3._V1_.jpg";

    [System.Diagnostics.CodeAnalysis.SuppressMessage("SonarLint", "S1075:URIs should not be hardcoded", Justification = "Seed data requires hardcoded URLs.")]
    private const string HungerGamesBookCoverUrl = "https://covers.openlibrary.org/b/isbn/9780439023481-L.jpg";
    [System.Diagnostics.CodeAnalysis.SuppressMessage("SonarLint", "S1075:URIs should not be hardcoded", Justification = "Seed data requires hardcoded URLs.")]
    private const string HungerGamesPosterUrl = "https://upload.wikimedia.org/wikipedia/uk/4/42/HungerGamesPoster.jpg";

    [System.Diagnostics.CodeAnalysis.SuppressMessage("SonarLint", "S1075:URIs should not be hardcoded", Justification = "Seed data requires hardcoded URLs.")]
    private const string MartianPosterUrl = "https://m.media-amazon.com/images/M/MV5BMTc2MTQ3MDA1Nl5BMl5BanBnXkFtZTgwODA3OTI4NjE@._V1_.jpg";
    [System.Diagnostics.CodeAnalysis.SuppressMessage("SonarLint", "S1075:URIs should not be hardcoded", Justification = "Seed data requires hardcoded URLs.")]
    private const string MartianBookCoverUrl = "https://covers.openlibrary.org/b/isbn/9780553418026-L.jpg";

    [System.Diagnostics.CodeAnalysis.SuppressMessage("SonarLint", "S1075:URIs should not be hardcoded", Justification = "Seed data requires hardcoded URLs.")]
    private const string GotPosterUrl = "https://images.weserv.nl/?url=https://www.impawards.com/tv/posters/game_of_thrones.jpg";
    [System.Diagnostics.CodeAnalysis.SuppressMessage("SonarLint", "S1075:URIs should not be hardcoded", Justification = "Seed data requires hardcoded URLs.")]
    private const string GotBookCoverUrl = "https://covers.openlibrary.org/b/isbn/9780553103540-L.jpg";

    [System.Diagnostics.CodeAnalysis.SuppressMessage("SonarLint", "S1075:URIs should not be hardcoded", Justification = "Seed data requires hardcoded URLs.")]
    private const string SherlockPosterUrl = "https://upload.wikimedia.org/wikipedia/uk/d/d4/Шерлок_Холмс_%28фільм%29.jpg";
    [System.Diagnostics.CodeAnalysis.SuppressMessage("SonarLint", "S1075:URIs should not be hardcoded", Justification = "Seed data requires hardcoded URLs.")]
    private const string SherlockBookCoverUrl = "https://covers.openlibrary.org/b/isbn/9781853260582-L.jpg";

    [System.Diagnostics.CodeAnalysis.SuppressMessage("SonarLint", "S1075:URIs should not be hardcoded", Justification = "Seed data requires hardcoded URLs.")]
    private const string ItPosterUrl = "https://upload.wikimedia.org/wikipedia/uk/b/b4/Воно_%28плакат_фільму%2C_2017%29.jpeg";
    [System.Diagnostics.CodeAnalysis.SuppressMessage("SonarLint", "S1075:URIs should not be hardcoded", Justification = "Seed data requires hardcoded URLs.")]
    private const string ItBookCoverUrl = "https://covers.openlibrary.org/b/isbn/9781501142970-L.jpg";

    [System.Diagnostics.CodeAnalysis.SuppressMessage("SonarLint", "S1075:URIs should not be hardcoded", Justification = "Seed data requires hardcoded URLs.")]
    private const string DaVinciPosterUrl = "https://upload.wikimedia.org/wikipedia/uk/0/06/%D0%9A%D0%BE%D0%B4_%D0%B4%D0%B0_%D0%92%D1%96%D0%BD%D1%87%D1%96.jpg";
    [System.Diagnostics.CodeAnalysis.SuppressMessage("SonarLint", "S1075:URIs should not be hardcoded", Justification = "Seed data requires hardcoded URLs.")]
    private const string DaVinciBookCoverUrl = "https://covers.openlibrary.org/b/isbn/9780307277671-L.jpg";

    [System.Diagnostics.CodeAnalysis.SuppressMessage("SonarLint", "S1075:URIs should not be hardcoded", Justification = "Seed data requires hardcoded URLs.")]
    private const string GodfatherPosterUrl = "https://m.media-amazon.com/images/M/MV5BM2MyNjYxNmUtYTAwNi00MTYxLWJmNWYtYzZlODY3ZTk3OTFlXkEyXkFqcGdeQXVyNzkwMjQ5NzM@._V1_.jpg";
    [System.Diagnostics.CodeAnalysis.SuppressMessage("SonarLint", "S1075:URIs should not be hardcoded", Justification = "Seed data requires hardcoded URLs.")]
    private const string GodfatherBookCoverUrl = "https://covers.openlibrary.org/b/isbn/9780451205766-L.jpg";

    [System.Diagnostics.CodeAnalysis.SuppressMessage("SonarLint", "S1075:URIs should not be hardcoded", Justification = "Seed data requires hardcoded URLs.")]
    private const string ShutterIslandBookCoverUrl = "https://images.weserv.nl/?url=https://images-na.ssl-images-amazon.com/images/S/compressed.photo.goodreads.com/books/1329269081l/21686.jpg";
    [System.Diagnostics.CodeAnalysis.SuppressMessage("SonarLint", "S1075:URIs should not be hardcoded", Justification = "Seed data requires hardcoded URLs.")]
    private const string ShutterIslandPosterUrl = "https://upload.wikimedia.org/wikipedia/uk/7/71/Острів_проклятих.jpg";

    [System.Diagnostics.CodeAnalysis.SuppressMessage("SonarLint", "S1075:URIs should not be hardcoded", Justification = "Seed data requires hardcoded URLs.")]
    private const string ForrestGumpBookCoverUrl = "https://images.weserv.nl/?url=https://images-na.ssl-images-amazon.com/images/P/0307947394.01.LZZZZZZZ.jpg";
    [System.Diagnostics.CodeAnalysis.SuppressMessage("SonarLint", "S1075:URIs should not be hardcoded", Justification = "Seed data requires hardcoded URLs.")]
    private const string ForrestGumpPosterUrl = "https://upload.wikimedia.org/wikipedia/uk/d/df/Forrest_Gump.jpg";

    [System.Diagnostics.CodeAnalysis.SuppressMessage("SonarLint", "S1075:URIs should not be hardcoded", Justification = "Seed data requires hardcoded URLs.")]
    private const string Orwell1984PosterUrl = "https://upload.wikimedia.org/wikipedia/uk/2/2e/1984_cover.jpg";
    [System.Diagnostics.CodeAnalysis.SuppressMessage("SonarLint", "S1075:URIs should not be hardcoded", Justification = "Seed data requires hardcoded URLs.")]
    private const string Orwell1984BookCoverUrl = "https://covers.openlibrary.org/b/isbn/9780451524935-L.jpg";

    [System.Diagnostics.CodeAnalysis.SuppressMessage("SonarLint", "S1075:URIs should not be hardcoded", Justification = "Seed data requires hardcoded URLs.")]
    private const string GatsbyPosterUrl = "https://m.media-amazon.com/images/M/MV5BMTkxNTk1ODcxNl5BMl5BanBnXkFtZTcwMDI1OTMzOQ@@._V1_.jpg";
    [System.Diagnostics.CodeAnalysis.SuppressMessage("SonarLint", "S1075:URIs should not be hardcoded", Justification = "Seed data requires hardcoded URLs.")]
    private const string GatsbyBookCoverUrl = "https://covers.openlibrary.org/b/isbn/9780743273565-L.jpg";

    [System.Diagnostics.CodeAnalysis.SuppressMessage("SonarLint", "S1075:URIs should not be hardcoded", Justification = "Seed data requires hardcoded URLs.")]
    private const string LordOfFliesPosterUrl = "https://images.weserv.nl/?url=https://www.impawards.com/1990/posters/lord_of_the_flies.jpg";
    [System.Diagnostics.CodeAnalysis.SuppressMessage("SonarLint", "S1075:URIs should not be hardcoded", Justification = "Seed data requires hardcoded URLs.")]
    private const string LordOfFliesBookCoverUrl = "https://covers.openlibrary.org/b/isbn/9780399501487-L.jpg";

    [System.Diagnostics.CodeAnalysis.SuppressMessage("SonarLint", "S1075:URIs should not be hardcoded", Justification = "Seed data requires hardcoded URLs for development avatars.")]
    private const string AdminAvatarUrl = "https://ui-avatars.com/api/?name=Admin&background=random";

    [System.Diagnostics.CodeAnalysis.SuppressMessage("SonarLint", "S1075:URIs should not be hardcoded", Justification = "Seed data requires hardcoded URLs for development avatars.")]
    private const string UserAvatarUrl = "https://ui-avatars.com/api/?name=John+Doe&background=random";

    [System.Diagnostics.CodeAnalysis.SuppressMessage("SonarLint", "S1075:URIs should not be hardcoded", Justification = "Seed data requires hardcoded URLs for development avatars.")]
    private const string CriticAvatarUrl = "https://ui-avatars.com/api/?name=Mike+Critic&background=random";

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
        User reviewer;

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

            reviewer = new User
            {
                Id = Guid.NewGuid(),
                Username = "critic_mike",
                Email = "mike@critics.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Critic123!"),
                Role = "user",
                AvatarUrl = CriticAvatarUrl,
                IsActive = true,
            };

            await context.Users.AddRangeAsync(admin, user, reviewer);
            await context.SaveChangesAsync();
        }
        else
        {
            admin = await context.Users.FirstAsync(u => u.Username == "admin");
            user = await context.Users.FirstAsync(u => u.Username == "john_doe");
            reviewer = await context.Users.FirstOrDefaultAsync(u => u.Username == "critic_mike") ?? user;
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
                CoverImageUrl = DuneBookCoverUrl,
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

            var reviews = new List<Review>
            {
                new Review
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    WorkId = work.Id,
                    TargetType = "comparison",
                    Text = "Фільм візуально приголомшливий, але книга пропонує набагато глибшу проробку світу. Спойлер: Пол виживає!",
                    IsSpoiler = false,
                    Rating = 9.0,
                    LikesCount = 10,
                },
                new Review
                {
                    Id = Guid.NewGuid(),
                    UserId = reviewer.Id,
                    WorkId = work.Id,
                    TargetType = "movie",
                    Text = "Дені Вільньов створив візуальний шедевр. Саундтрек Ганса Ціммера просто неймовірний.",
                    IsSpoiler = false,
                    Rating = 9.5,
                    LikesCount = 42,
                },
            };
            await context.Reviews.AddRangeAsync(reviews);

            var report = new Report
            {
                Id = Guid.NewGuid(),
                UserId = admin.Id,
                ReviewId = reviews[0].Id,
                Reason = "Містить приховані спойлери без відповідного тегу",
                Status = "Pending",
            };
            await context.Reports.AddAsync(report);
        }

        // 3. Додатковий твір (Володар Перснів)
        var lotrWork = await context.Works
            .Include(w => w.Book)
            .Include(w => w.Adaptation)
            .Include(w => w.Rating)
            .Include(w => w.DifferenceMap)
            .FirstOrDefaultAsync(w => w.Title == "Володар Перснів (2001)");

        if (lotrWork == null)
        {
            var author = new Author { Id = Guid.NewGuid(), FullName = "Дж. Р. Р. Толкін", Nationality = "Британець" };
            var actor = new Actor { Id = Guid.NewGuid(), FullName = "Елайджа Вуд", Nationality = "Американець" };

            var book = new Book
            {
                Id = Guid.NewGuid(),
                Title = "Братство Персня",
                Genre = "Фентезі",
                PublicationYear = 1954,
                CoverImageUrl = LotrBookCoverUrl,
                Authors = new List<Author> { author },
            };

            var adaptation = new Adaptation
            {
                Id = Guid.NewGuid(),
                Title = "Володар перснів: Хранителі персня",
                Type = "movie",
                ReleaseYear = 2001,
                Country = "Нова Зеландія, США",
                PosterUrl = LotrPosterUrl,
            };

            lotrWork = new Work
            {
                Id = Guid.NewGuid(),
                Book = book,
                Adaptation = adaptation,
                Title = "Володар Перснів (2001)",
                Summary = "Культова трилогія Пітера Джексона.",
            };

            var adaptationActor = new AdaptationActor { Adaptation = adaptation, Actor = actor, RoleName = "Фродо Беггінс", };

            await context.Works.AddAsync(lotrWork);
            await context.Set<AdaptationActor>().AddAsync(adaptationActor);

            // Додаємо в обране для користувача
            await context.Favorites.AddAsync(new Favorite { UserId = user.Id, WorkId = lotrWork.Id, Kind = "read", });

            var lotrReview = new Review
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                WorkId = lotrWork.Id,
                TargetType = "comparison",
                Text = "Неймовірна екранізація! Хоча шкода, що вирізали Тома Бомбадила, епічність фільму компенсує це на 100%.",
                IsSpoiler = false,
                Rating = 9.5,
                LikesCount = 25,
            };
            await context.Reviews.AddAsync(lotrReview);
        }

        if (lotrWork.Rating == null)
        {
            var rating = new Rating
            {
                Id = Guid.NewGuid(),
                WorkId = lotrWork.Id,
                BookRating = 9.8m,
                AdaptationRating = 9.1m,
                VotesCount = 1,
            };
            await context.Ratings.AddAsync(rating);
        }

        if (lotrWork.DifferenceMap == null)
        {
            var map = new DifferenceMap
            {
                Id = Guid.NewGuid(),
                WorkId = lotrWork.Id,
                Title = "Мапа розбіжностей: Володар Перснів",
                Differences = new List<Difference>
                {
                    new Difference
                    {
                        Id = Guid.NewGuid(),
                        Title = "Бомбадил вирізаний з екранізації",
                        BookText = "Том Бомбадил зустрічає Фродо, Сема, Меррі та Піппіна у Старому лісі, рятує їх від Старої Верби та Могильників, і приймає їх у себе вдома.",
                        FilmText = "Том Бомбадил повністю відсутній в екранізації. Гобіти проходять ліс без зустрічі з ним.",
                        ImportanceLevel = "medium",
                        IsSpoiler = false,
                    },
                },
            };
            await context.DifferenceMaps.AddAsync(map);
        }

        // 4. Гаррі Поттер і філософський камінь
        var hpWork = await context.Works
            .Include(w => w.Book)
            .Include(w => w.Adaptation)
            .Include(w => w.Rating)
            .Include(w => w.DifferenceMap)
            .FirstOrDefaultAsync(w => w.Title == "Гаррі Поттер і філософський камінь");

        if (hpWork == null)
        {
            var author = await context.Authors.FirstOrDefaultAsync(a => a.FullName == "Джоан Роулінг");
            if (author == null)
            {
                author = new Author { Id = Guid.NewGuid(), FullName = "Джоан Роулінг", Nationality = "Британка" };
                await context.Authors.AddAsync(author);
            }

            var book = new Book
            {
                Id = Guid.NewGuid(),
                Title = "Гаррі Поттер і філософський камінь",
                Description = "На початку роману світ чарівників святкує поразку Лорда Волдеморта, можутьнього й жорстокого злого чарівника. Вбивши Лілі та Джеймса Поттерів, Волдеморт намагається покінчити з їхнім однорічним сином Гаррі, але закляття смерті обертається проти нього самого, знищивши його тіло й залишивши шрам у формі блискавки на чолі дитини. Професори Албус Дамблдор і Мінерва Макґонеґел, а також лісник Рубеус Геґрід залишають Гаррі коло дверей будинку його родичів-маґлів. Тітка Гаррі Петунія Дурслі є сестрою Лілі Поттер, хоча вона ніколи не почувала симпатії до чарівниці. Дурслі вирішують приховати від Гаррі правду про його батьків, переконавши його, що ті загинули в автокатастрофі. Протягом всього дитинства вони поводяться з ним украй неласкаво, водночас приділяючи надмірну увагу своєму синові Дадлі.",
                Genre = "Фентезі",
                PublicationYear = 1997,
                Language = "Українська",
                CoverImageUrl = HpBookCoverUrl,
                Authors = new List<Author> { author },
            };

            var adaptation = new Adaptation
            {
                Id = Guid.NewGuid(),
                Title = "Гаррі Поттер і філософський камінь",
                Type = "movie",
                Description = "Одинадцятирічний Гаррі Поттер, що втратив своїх батьків у ранньому дитинстві, живе з дядьком і тіткою. Він змушений терпіти їхнє погане ставлення до себе й жити под сходами. Але один раз, хлопчик одержує запрошення вчитися в Гоґвортсі — школі для юних чарівників, де вчать чаклунству й магії. Виявляється, батьки Гаррі були чарівниками, але їх убив злий чаклун, що тепер намагається проникнути в Гоґвортс, щоб украсти захований там філософський камінь. Незважаючи на протести свого злого сімейства, Гаррі відправляється в школу, де буде вчитися чарівництву. Там він знайде нових цікавих друзів, які допоможуть йому довідатися правду про батьків.",
                ReleaseYear = 2001,
                DurationMinutes = 152,
                Studio = "Кріс Коламбус",
                Country = "Велика Британія",
                PosterUrl = HarryPotterPosterUrl,
            };

            hpWork = new Work
            {
                Id = Guid.NewGuid(),
                Book = book,
                Adaptation = adaptation,
                Title = "Гаррі Поттер і філософський камінь",
                Summary = "Порівняння першого роману Джоан Роулінг та його екранізації від Кріса Коламбуса.",
            };

            await context.Works.AddAsync(hpWork);

            var actorDaniel = await context.Actors.FirstOrDefaultAsync(a => a.FullName == "Деніел Редкліфф");
            if (actorDaniel == null)
            {
                actorDaniel = new Actor { Id = Guid.NewGuid(), FullName = "Деніел Редкліфф", Nationality = "Британець" };
                await context.Actors.AddAsync(actorDaniel);
            }

            var actorEmma = await context.Actors.FirstOrDefaultAsync(a => a.FullName == "Емма Вотсон");
            if (actorEmma == null)
            {
                actorEmma = new Actor { Id = Guid.NewGuid(), FullName = "Емма Вотсон", Nationality = "Британка" };
                await context.Actors.AddAsync(actorEmma);
            }

            var actorRupert = await context.Actors.FirstOrDefaultAsync(a => a.FullName == "Руперт Ґрінт");
            if (actorRupert == null)
            {
                actorRupert = new Actor { Id = Guid.NewGuid(), FullName = "Руперт Ґрінт", Nationality = "Британець" };
                await context.Actors.AddAsync(actorRupert);
            }

            var adaptationActor1 = new AdaptationActor { Adaptation = adaptation, Actor = actorDaniel, RoleName = "Гаррі Поттер" };
            var adaptationActor2 = new AdaptationActor { Adaptation = adaptation, Actor = actorEmma, RoleName = "Герміона Ґрейнджер" };
            var adaptationActor3 = new AdaptationActor { Adaptation = adaptation, Actor = actorRupert, RoleName = "Рон Візлі" };

            await context.Set<AdaptationActor>().AddRangeAsync(adaptationActor1, adaptationActor2, adaptationActor3);

            var hpReview = new Review
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                WorkId = hpWork.Id,
                TargetType = "comparison",
                Text = "Чудовий дитячий фільм, який чудово передає атмосферу чарівництва з книги. Перший фільм дуже близький до тексту.",
                IsSpoiler = false,
                Rating = 9.0,
                LikesCount = 18,
            };
            await context.Reviews.AddAsync(hpReview);
        }

        if (hpWork.Rating == null)
        {
            var rating = new Rating
            {
                Id = Guid.NewGuid(),
                WorkId = hpWork.Id,
                BookRating = 9.2m,
                AdaptationRating = 8.5m,
                VotesCount = 1,
            };
            await context.Ratings.AddAsync(rating);
        }

        if (hpWork.DifferenceMap == null)
        {
            var map = new DifferenceMap
            {
                Id = Guid.NewGuid(),
                WorkId = hpWork.Id,
                Title = "Мапа розбіжностей: Гаррі Поттер і філософський камінь",
                Differences = new List<Difference>
                {
                    new Difference
                    {
                        Id = Guid.NewGuid(),
                        Title = "Перше знайомство з Драко Малфоєм",
                        BookText = "Гаррі знайомиться з Драко ще до школи — у магазині мантій Madam Malkin's. Це знайомство показує характер Малфоя ще до потрапяння в Гоґвортс.",
                        FilmText = "У фільмі ця сцена відсутня. Перше знайомство відбувається вже у замку, коли Гаррі та Рон приходять до великої зали.",
                        ImportanceLevel = "medium",
                        IsSpoiler = false,
                    },
                    new Difference
                    {
                        Id = Guid.NewGuid(),
                        Title = "Святковий бенкет на честь початку навчального року",
                        BookText = "У книзі багато діалогів між учнями про Поттера, і Гаррі сидить у напівтемряві.",
                        FilmText = "У фільмі ефектне сповільнене панорамування над великою залою.",
                        ImportanceLevel = "low",
                        IsSpoiler = false,
                    },
                    new Difference
                    {
                        Id = Guid.NewGuid(),
                        Title = "Зустріч із Норбертом",
                        BookText = "Гаррі, Рон і Герміона довго возилися із Норбертом — драконом Гаґріда. Цей сюжет займає кілька розділів.",
                        FilmText = "Норберта прибрали з фільму повністю — щоб скоротити хронометраж.",
                        ImportanceLevel = "high",
                        IsSpoiler = true,
                    },
                    new Difference
                    {
                        Id = Guid.NewGuid(),
                        Title = "Дзеркало Яцрес",
                        BookText = "Описано детально, як Гаррі провів багато ночей перед дзеркалом, бачачи свою сім'ю.",
                        FilmText = "У фільмі — одна сцена з матір'ю та батьком, без розгорнутої лінії.",
                        ImportanceLevel = "medium",
                        IsSpoiler = false,
                    },
                    new Difference
                    {
                        Id = Guid.NewGuid(),
                        Title = "Фінальна боротьба з Кваррелом",
                        BookText = "У книзі Гаррі сам спалює Кваррела дотиком долонь — Волдеморт відступає.",
                        FilmText = "У фільмі сцена показана більш візуально — обличчя Кваррела розсипається на попіл.",
                        ImportanceLevel = "high",
                        IsSpoiler = true,
                    },
                },
            };
            await context.DifferenceMaps.AddAsync(map);
        }

        // 5. Бійцівський клуб (Fight Club)
        if (!await context.Works.AnyAsync(w => w.Title == "Бійцівський клуб"))
        {
            var author = new Author { Id = Guid.NewGuid(), FullName = "Чак Поланік", Nationality = "Американець" };
            var actor1 = new Actor { Id = Guid.NewGuid(), FullName = "Бред Пітт", Nationality = "Американець" };
            var actor2 = new Actor { Id = Guid.NewGuid(), FullName = "Едвард Нортон", Nationality = "Американець" };

            var book = new Book
            {
                Id = Guid.NewGuid(),
                Title = "Бійцівський клуб",
                Description = "Сатиричний роман про споживацтво та нігілізм.",
                Genre = "Контркультура",
                PublicationYear = 1996,
                CoverImageUrl = FightClubBookCoverUrl,
                Authors = new List<Author> { author },
            };

            var adaptation = new Adaptation
            {
                Id = Guid.NewGuid(),
                Title = "Бійцівський клуб",
                Type = "movie",
                ReleaseYear = 1999,
                Country = "США",
                PosterUrl = FightClubPosterUrl,
            };

            var work = new Work
            {
                Id = Guid.NewGuid(),
                Book = book,
                Adaptation = adaptation,
                Title = "Бійцівський клуб",
                Summary = "Культова екранізація Девіда Фінчера.",
            };

            await context.Works.AddAsync(work);
            await context.Set<AdaptationActor>().AddRangeAsync(
                new AdaptationActor { Adaptation = adaptation, Actor = actor1, RoleName = "Тайлер Дерден" },
                new AdaptationActor { Adaptation = adaptation, Actor = actor2, RoleName = "Оповідач" });

            await context.Ratings.AddAsync(new Rating { Id = Guid.NewGuid(), WorkId = work.Id, BookRating = 8.8m, AdaptationRating = 9.2m, VotesCount = 1 });

            await context.DifferenceMaps.AddAsync(new DifferenceMap
            {
                Id = Guid.NewGuid(),
                WorkId = work.Id,
                Title = "Мапа розбіжностей: Бійцівський клуб",
                Differences = new List<Difference>
                {
                    new Difference { Id = Guid.NewGuid(), Title = "Кінцівка", BookText = "Головний герой опиняється в психіатричній лікарні, яку він сприймає як рай.", FilmText = "Головний герой стоїть з Марлою і спостерігає за руйнуванням хмарочосів під 'Where Is My Mind?'.", ImportanceLevel = "high", IsSpoiler = true },
                },
            });
        }

        // 6. Відьмак (The Witcher)
        if (!await context.Works.AnyAsync(w => w.Title == "Відьмак (Серіал)"))
        {
            var author = new Author { Id = Guid.NewGuid(), FullName = "Анджей Сапковський", Nationality = "Поляк" };
            var actor = new Actor { Id = Guid.NewGuid(), FullName = "Генрі Кавілл", Nationality = "Британець" };

            var book = new Book
            {
                Id = Guid.NewGuid(),
                Title = "Останнє бажання",
                Genre = "Фентезі",
                PublicationYear = 1993,
                CoverImageUrl = WitcherBookCoverUrl,
                Authors = new List<Author> { author },
            };

            var adaptation = new Adaptation
            {
                Id = Guid.NewGuid(),
                Title = "Відьмак (1 сезон)",
                Type = "series",
                ReleaseYear = 2019,
                Country = "США, Польща",
                PosterUrl = WitcherPosterUrl,
            };

            var work = new Work
            {
                Id = Guid.NewGuid(),
                Book = book,
                Adaptation = adaptation,
                Title = "Відьмак (Серіал)",
                Summary = "Екранізація оповідань Сапковського від Netflix.",
            };

            await context.Works.AddAsync(work);
            await context.Set<AdaptationActor>().AddAsync(new AdaptationActor { Adaptation = adaptation, Actor = actor, RoleName = "Ґеральт із Рівії" });
            await context.Ratings.AddAsync(new Rating { Id = Guid.NewGuid(), WorkId = work.Id, BookRating = 9.3m, AdaptationRating = 7.8m, VotesCount = 1 });
        }

        // 7. Марсіянин (The Martian)
        if (!await context.Works.AnyAsync(w => w.Title == "Марсіянин"))
        {
            var author = new Author { Id = Guid.NewGuid(), FullName = "Енді Вейр", Nationality = "Американець" };
            var actor = new Actor { Id = Guid.NewGuid(), FullName = "Метт Деймон", Nationality = "Американець" };

            var book = new Book
            {
                Id = Guid.NewGuid(),
                Title = "Марсіянин",
                Genre = "Наукова фантастика",
                PublicationYear = 2011,
                CoverImageUrl = MartianBookCoverUrl,
                Authors = new List<Author> { author },
            };

            var adaptation = new Adaptation
            {
                Id = Guid.NewGuid(),
                Title = "Марсіянин",
                Type = "movie",
                ReleaseYear = 2015,
                Country = "США",
                PosterUrl = MartianPosterUrl,
            };

            var work = new Work
            {
                Id = Guid.NewGuid(),
                Book = book,
                Adaptation = adaptation,
                Title = "Марсіянин",
                Summary = "Захоплива історія виживання астронавта на Марсі.",
            };

            await context.Works.AddAsync(work);
            await context.Set<AdaptationActor>().AddAsync(new AdaptationActor { Adaptation = adaptation, Actor = actor, RoleName = "Марк Вотні" });
            await context.Ratings.AddAsync(new Rating { Id = Guid.NewGuid(), WorkId = work.Id, BookRating = 9.2m, AdaptationRating = 8.8m, VotesCount = 1 });

            await context.DifferenceMaps.AddAsync(new DifferenceMap
            {
                Id = Guid.NewGuid(),
                WorkId = work.Id,
                Title = "Мапа розбіжностей: Марсіянин",
                Differences = new List<Difference>
                {
                    new Difference { Id = Guid.NewGuid(), Title = "Внутрішній монолог", BookText = "Велика частина книги — це детальні наукові розрахунки та гумористичний внутрішній монолог Вотні.", FilmText = "Фільм фокусується на візуальній драмі, скорочуючи складні математичні пояснення.", ImportanceLevel = "medium" },
                    new Difference { Id = Guid.NewGuid(), Title = "Кінцівка рятування", BookText = "Вотні не використовує Залізну людину (проколювання рукавички) для маневрування.", FilmText = "Вотні проколює рукавичку скафандра, щоб долетіти до рятувального модуля.", ImportanceLevel = "high", IsSpoiler = true },
                },
            });
        }

        // 8. Гра престолів (Game of Thrones)
        if (!await context.Works.AnyAsync(w => w.Title == "Гра престолів"))
        {
            var author = new Author { Id = Guid.NewGuid(), FullName = "Джордж Р. Р. Мартін", Nationality = "Американець" };

            var book = new Book
            {
                Id = Guid.NewGuid(),
                Title = "Гра престолів",
                Genre = "Фентезі",
                PublicationYear = 1996,
                CoverImageUrl = GotBookCoverUrl,
                Authors = new List<Author> { author },
            };

            var adaptation = new Adaptation
            {
                Id = Guid.NewGuid(),
                Title = "Гра престолів (серіал)",
                Type = "series",
                ReleaseYear = 2011,
                Country = "США, Велика Британія",
                PosterUrl = GotPosterUrl,
            };

            var work = new Work
            {
                Id = Guid.NewGuid(),
                Book = book,
                Adaptation = adaptation,
                Title = "Гра престолів",
                Summary = "Епічне фентезі за мотивами циклу 'Пісня льоду й полум'я'.",
            };

            await context.Works.AddAsync(work);
            await context.Ratings.AddAsync(new Rating { Id = Guid.NewGuid(), WorkId = work.Id, BookRating = 9.5m, AdaptationRating = 9.3m, VotesCount = 1 });

            await context.DifferenceMaps.AddAsync(new DifferenceMap
            {
                Id = Guid.NewGuid(),
                WorkId = work.Id,
                Title = "Мапа розбіжностей: Гра престолів",
                Differences = new List<Difference>
                {
                    new Difference { Id = Guid.NewGuid(), Title = "Вік персонажів", BookText = "На початку книги Джону Сноу та Роббу Старку по 14 років, Деенерис — 13.", FilmText = "У серіалі всі підлітки значно старші (близько 17-18 років).", ImportanceLevel = "medium" },
                    new Difference { Id = Guid.NewGuid(), Title = "Зовнішність Тиріона", BookText = "Після битви на Чорноводній Тиріон втрачає більшу частину носа і стає понівеченим.", FilmText = "У Тиріона лише великий шрам на обличчі, він залишається відносно привабливим.", ImportanceLevel = "low" },
                },
            });
        }

        // 9. Шерлок Голмс (Sherlock Holmes)
        if (!await context.Works.AnyAsync(w => w.Title == "Шерлок Голмс"))
        {
            var author = new Author { Id = Guid.NewGuid(), FullName = "Артур Конан Дойл", Nationality = "Британець" };

            var book = new Book
            {
                Id = Guid.NewGuid(),
                Title = "Етюд у багрових тонах",
                Genre = "Детектив",
                PublicationYear = 1887,
                CoverImageUrl = SherlockBookCoverUrl,
                Authors = new List<Author> { author },
            };

            var adaptation = new Adaptation
            {
                Id = Guid.NewGuid(),
                Title = "Шерлок (серіал BBC)",
                Type = "series",
                ReleaseYear = 2010,
                Country = "Велика Британія",
                PosterUrl = SherlockPosterUrl,
            };

            var work = new Work
            {
                Id = Guid.NewGuid(),
                Book = book,
                Adaptation = adaptation,
                Title = "Шерлок Голмс",
                Summary = "Сучасна інтерпретація класичного детектива.",
            };

            await context.Works.AddAsync(work);
            await context.Ratings.AddAsync(new Rating { Id = Guid.NewGuid(), WorkId = work.Id, BookRating = 9.4m, AdaptationRating = 9.1m, VotesCount = 1 });

            await context.DifferenceMaps.AddAsync(new DifferenceMap
            {
                Id = Guid.NewGuid(),
                WorkId = work.Id,
                Title = "Мапа розбіжностей: Шерлок",
                Differences = new List<Difference>
                {
                    new Difference { Id = Guid.NewGuid(), Title = "Епоха", BookText = "Дія відбувається наприкінці 19 століття у вікторіанському Лондоні.", FilmText = "Події перенесені у сучасний Лондон 21 століття з використанням смартфонів та блогів.", ImportanceLevel = "high" },
                    new Difference { Id = Guid.NewGuid(), Title = "Метод Ватсона", BookText = "Ватсон пише оповідання про пригоди для журналів.", FilmText = "Ватсон веде інтернет-блог про розслідування Шерлока.", ImportanceLevel = "medium" },
                },
            });
        }

        // 10. Воно (It)
        if (!await context.Works.AnyAsync(w => w.Title == "Воно"))
        {
            var author = new Author { Id = Guid.NewGuid(), FullName = "Стівен Кінґ", Nationality = "Американець" };

            var book = new Book
            {
                Id = Guid.NewGuid(),
                Title = "Воно",
                Genre = "Жахи",
                PublicationYear = 1986,
                CoverImageUrl = ItBookCoverUrl,
                Authors = new List<Author> { author },
            };

            var adaptation = new Adaptation
            {
                Id = Guid.NewGuid(),
                Title = "Воно (2017)",
                Type = "movie",
                ReleaseYear = 2017,
                Country = "США",
                PosterUrl = ItPosterUrl,
            };

            var work = new Work
            {
                Id = Guid.NewGuid(),
                Book = book,
                Adaptation = adaptation,
                Title = "Воно",
                Summary = "Екранізація культового роману Стівена Кінґа.",
            };

            await context.Works.AddAsync(work);
            await context.Ratings.AddAsync(new Rating { Id = Guid.NewGuid(), WorkId = work.Id, BookRating = 9.0m, AdaptationRating = 8.5m, VotesCount = 1 });

            await context.DifferenceMaps.AddAsync(new DifferenceMap
            {
                Id = Guid.NewGuid(),
                WorkId = work.Id,
                Title = "Мапа розбіжностей: Воно",
                Differences = new List<Difference>
                {
                    new Difference { Id = Guid.NewGuid(), Title = "Часові рамки", BookText = "Дитинство героїв припадає на 1950-ті роки.", FilmText = "Дія першої частини фільму перенесена у 1980-ті роки.", ImportanceLevel = "medium" },
                    new Difference { Id = Guid.NewGuid(), Title = "Ритуал Чу", BookText = "Для перемоги над Воно діти використовують складний метафізичний ритуал Чу.", FilmText = "Діти перемагають Воно через фізичну боротьбу та подолання власного страху.", ImportanceLevel = "high", IsSpoiler = true },
                },
            });
        }

        // 11. Код да Вінчі (The Da Vinci Code)
        if (!await context.Works.AnyAsync(w => w.Title == "Код да Вінчі"))
        {
            var author = new Author { Id = Guid.NewGuid(), FullName = "Ден Браун", Nationality = "Американець" };

            var book = new Book
            {
                Id = Guid.NewGuid(),
                Title = "Код да Вінчі",
                Genre = "Трилер",
                PublicationYear = 2003,
                CoverImageUrl = DaVinciBookCoverUrl,
                Authors = new List<Author> { author },
            };

            var adaptation = new Adaptation
            {
                Id = Guid.NewGuid(),
                Title = "Код да Вінчі (фільм)",
                Type = "movie",
                ReleaseYear = 2006,
                Country = "США",
                PosterUrl = DaVinciPosterUrl,
            };

            var work = new Work
            {
                Id = Guid.NewGuid(),
                Book = book,
                Adaptation = adaptation,
                Title = "Код да Вінчі",
                Summary = "Інтелектуальний трилер про пошуки Святого Ґрааля.",
            };

            await context.Works.AddAsync(work);
            await context.Ratings.AddAsync(new Rating { Id = Guid.NewGuid(), WorkId = work.Id, BookRating = 8.5m, AdaptationRating = 8.2m, VotesCount = 1 });

            await context.DifferenceMaps.AddAsync(new DifferenceMap
            {
                Id = Guid.NewGuid(),
                WorkId = work.Id,
                Title = "Мапа розбіжностей: Код да Вінчі",
                Differences = new List<Difference>
                {
                    new Difference { Id = Guid.NewGuid(), Title = "Походження Ленґдона", BookText = "Ленґдон детально пояснює символіку через свої спогади та академічний досвід.", FilmText = "Багато пояснень спрощено або візуалізовано через швидкі вставки-флешбеки.", ImportanceLevel = "low" },
                    new Difference { Id = Guid.NewGuid(), Title = "Фінальне одкровення", BookText = "Софі дізнається про свою родину в каплиці Рослін через зустріч з бабусею та братом.", FilmText = "Сцена в каплиці скорочена, акцент зроблено на емоційному усвідомленні Софі.", ImportanceLevel = "medium", IsSpoiler = true },
                },
            });
        }

        // 12. Хрещений батько (The Godfather)
        if (!await context.Works.AnyAsync(w => w.Title == "Хрещений батько"))
        {
            var author = new Author { Id = Guid.NewGuid(), FullName = "Маріо П'юзо", Nationality = "Американець" };

            var book = new Book
            {
                Id = Guid.NewGuid(),
                Title = "Хрещений батько",
                Genre = "Кримінал",
                PublicationYear = 1969,
                CoverImageUrl = GodfatherBookCoverUrl,
                Authors = new List<Author> { author },
            };

            var adaptation = new Adaptation
            {
                Id = Guid.NewGuid(),
                Title = "Хрещений батько (1972)",
                Type = "movie",
                ReleaseYear = 1972,
                Country = "США",
                PosterUrl = GodfatherPosterUrl,
            };

            var work = new Work
            {
                Id = Guid.NewGuid(),
                Book = book,
                Adaptation = adaptation,
                Title = "Хрещений батько",
                Summary = "Класика кримінальної драми про сім'ю Корлеоне.",
            };

            await context.Works.AddAsync(work);
            await context.Ratings.AddAsync(new Rating { Id = Guid.NewGuid(), WorkId = work.Id, BookRating = 9.7m, AdaptationRating = 9.8m, VotesCount = 1 });

            await context.DifferenceMaps.AddAsync(new DifferenceMap
            {
                Id = Guid.NewGuid(),
                WorkId = work.Id,
                Title = "Мапа розбіжностей: Хрещений батько",
                Differences = new List<Difference>
                {
                    new Difference { Id = Guid.NewGuid(), Title = "Лінія Джонні Фонтейна", BookText = "Книга приділяє багато уваги кар'єрі співака Джонні Фонтейна у Голлівуді.", FilmText = "Роль Джонні Фонтейна у фільмі епізодична, його лінія майже повністю вирізана.", ImportanceLevel = "medium" },
                    new Difference { Id = Guid.NewGuid(), Title = "Минуле Віто Корлеоне", BookText = "Історія становлення Віто Корлеоне вплетена в основний сюжет першої книги.", FilmText = "Ця частина була перенесена у другий фільм ('Хрещений батько 2').", ImportanceLevel = "high" },
                },
            });
        }

        // 13. 1984 (Nineteen Eighty-Four)
        if (!await context.Works.AnyAsync(w => w.Title == "1984"))
        {
            var author = new Author { Id = Guid.NewGuid(), FullName = "Джордж Орвелл", Nationality = "Британець" };

            var book = new Book
            {
                Id = Guid.NewGuid(),
                Title = "1984",
                Genre = "Антиутопія",
                PublicationYear = 1949,
                CoverImageUrl = Orwell1984BookCoverUrl,
                Authors = new List<Author> { author },
            };

            var adaptation = new Adaptation
            {
                Id = Guid.NewGuid(),
                Title = "1984 (фільм)",
                Type = "movie",
                ReleaseYear = 1984,
                Country = "Велика Британія",
                PosterUrl = Orwell1984PosterUrl,
            };

            var work = new Work
            {
                Id = Guid.NewGuid(),
                Book = book,
                Adaptation = adaptation,
                Title = "1984",
                Summary = "Похмура антиутопія про тоталітарне суспільство та Великого Брата.",
            };

            await context.Works.AddAsync(work);
            await context.Ratings.AddAsync(new Rating { Id = Guid.NewGuid(), WorkId = work.Id, BookRating = 9.6m, AdaptationRating = 8.7m, VotesCount = 1 });

            await context.DifferenceMaps.AddAsync(new DifferenceMap
            {
                Id = Guid.NewGuid(),
                WorkId = work.Id,
                Title = "Мапа розбіжностей: 1984",
                Differences = new List<Difference>
                {
                    new Difference { Id = Guid.NewGuid(), Title = "Атмосфера та колір", BookText = "Вінстон сприймає світ як сірий та занедбаний, що передається через текст.", FilmText = "Фільм використовує вицвілу кольорову гаму, щоб візуально передати безнадію.", ImportanceLevel = "low" },
                    new Difference { Id = Guid.NewGuid(), Title = "Роль щоденника", BookText = "У книзі ми читаємо багато роздумів Вінстона, які він записує у щоденник.", FilmText = "Щоденник показаний як фізичний об'єкт, але внутрішні роздуми значно скорочені.", ImportanceLevel = "medium" },
                },
            });
        }

        // 14. Великий Гетсбі (The Great Gatsby)
        if (!await context.Works.AnyAsync(w => w.Title == "Великий Гетсбі"))
        {
            var author = new Author { Id = Guid.NewGuid(), FullName = "Френсіс Скотт Фіцджеральд", Nationality = "Американець" };

            var book = new Book
            {
                Id = Guid.NewGuid(),
                Title = "Великий Гетсбі",
                Genre = "Класика",
                PublicationYear = 1925,
                CoverImageUrl = GatsbyBookCoverUrl,
                Authors = new List<Author> { author },
            };

            var adaptation = new Adaptation
            {
                Id = Guid.NewGuid(),
                Title = "Великий Гетсбі (2013)",
                Type = "movie",
                ReleaseYear = 2013,
                Country = "Австралія, США",
                PosterUrl = GatsbyPosterUrl,
            };

            var work = new Work
            {
                Id = Guid.NewGuid(),
                Book = book,
                Adaptation = adaptation,
                Title = "Великий Гетсбі",
                Summary = "Історія про американську мрію, кохання та трагедію в епоху джазу.",
            };

            await context.Works.AddAsync(work);
            await context.Ratings.AddAsync(new Rating { Id = Guid.NewGuid(), WorkId = work.Id, BookRating = 9.1m, AdaptationRating = 8.4m, VotesCount = 1 });

            await context.DifferenceMaps.AddAsync(new DifferenceMap
            {
                Id = Guid.NewGuid(),
                WorkId = work.Id,
                Title = "Мапа розбіжностей: Великий Гетсбі",
                Differences = new List<Difference>
                {
                    new Difference { Id = Guid.NewGuid(), Title = "Рамкова історія", BookText = "Нік Керравей розповідає історію як спогади, перебуваючи вдома на Середньому Заході.", FilmText = "Нік перебуває в санаторії і пише історію як частину терапії.", ImportanceLevel = "medium" },
                    new Difference { Id = Guid.NewGuid(), Title = "Музичний супровід", BookText = "Атмосфера джазових вечірок 1920-х років.", FilmText = "Використання сучасної музики (хіп-хоп) у поєднанні з естетикою 20-х.", ImportanceLevel = "low" },
                },
            });
        }

        // 15. Володар мух (Lord of the Flies)
        if (!await context.Works.AnyAsync(w => w.Title == "Володар мух"))
        {
            var author = new Author { Id = Guid.NewGuid(), FullName = "Вільям Ґолдінґ", Nationality = "Британець" };

            var book = new Book
            {
                Id = Guid.NewGuid(),
                Title = "Володар мух",
                Genre = "Алегоричний роман",
                PublicationYear = 1954,
                CoverImageUrl = LordOfFliesBookCoverUrl,
                Authors = new List<Author> { author },
            };

            var adaptation = new Adaptation
            {
                Id = Guid.NewGuid(),
                Title = "Володар мух (1990)",
                Type = "movie",
                ReleaseYear = 1990,
                Country = "США",
                PosterUrl = LordOfFliesPosterUrl,
            };

            var work = new Work
            {
                Id = Guid.NewGuid(),
                Book = book,
                Adaptation = adaptation,
                Title = "Володар мух",
                Summary = "Жорстока історія про групу хлопчиків, що опинилися на безлюдному острові.",
            };

            await context.Works.AddAsync(work);
            await context.Ratings.AddAsync(new Rating { Id = Guid.NewGuid(), WorkId = work.Id, BookRating = 8.9m, AdaptationRating = 7.5m, VotesCount = 1 });

            await context.DifferenceMaps.AddAsync(new DifferenceMap
            {
                Id = Guid.NewGuid(),
                WorkId = work.Id,
                Title = "Мапа розбіжностей: Володар мух",
                Differences = new List<Difference>
                {
                    new Difference { Id = Guid.NewGuid(), Title = "Національність", BookText = "Хлопчики — вихованці британської школи.", FilmText = "Хлопчики — кадети американської військової академії.", ImportanceLevel = "medium" },
                    new Difference { Id = Guid.NewGuid(), Title = "Кінцівка (рятування)", BookText = "Хлопчиків знаходить офіцер британського флоту.", FilmText = "Морські піхотинці США знаходять хлопчиків на пляжі.", ImportanceLevel = "low" },
                },
            });
        }

        // 16. Спіймати Кайдаша (Кайдашева сім'я)
        if (!await context.Works.AnyAsync(w => w.Title == "Спіймати Кайдаша"))
        {
            var author = new Author { Id = Guid.NewGuid(), FullName = "Іван Нечуй-Левицький", Nationality = "Українець" };

            var book = new Book
            {
                Id = Guid.NewGuid(),
                Title = "Кайдашева сім'я",
                Genre = "Класика",
                PublicationYear = 1878,
                CoverImageUrl = KaidashBookCoverUrl,
                Authors = new List<Author> { author },
            };

            var adaptation = new Adaptation
            {
                Id = Guid.NewGuid(),
                Title = "Спіймати Кайдаша",
                Type = "series",
                ReleaseYear = 2020,
                Country = "Україна",
                PosterUrl = KaidashPosterUrl,
            };

            var work = new Work
            {
                Id = Guid.NewGuid(),
                Book = book,
                Adaptation = adaptation,
                Title = "Спіймати Кайдаша",
                Summary = "Сучасна інтерпретація класичної повісті Нечуя-Левицького.",
            };

            await context.Works.AddAsync(work);
            await context.Ratings.AddAsync(new Rating { Id = Guid.NewGuid(), WorkId = work.Id, BookRating = 9.8m, AdaptationRating = 9.9m, VotesCount = 1 });

            await context.DifferenceMaps.AddAsync(new DifferenceMap
            {
                Id = Guid.NewGuid(),
                WorkId = work.Id,
                Title = "Мапа розбіжностей: Спіймати Кайдаша",
                Differences = new List<Difference>
                {
                    new Difference { Id = Guid.NewGuid(), Title = "Часові рамки", BookText = "Події відбуваються у 19 столітті після скасування кріпацтва.", FilmText = "Події перенесені у сучасну Україну (2005-2014 роки).", ImportanceLevel = "high" },
                    new Difference { Id = Guid.NewGuid(), Title = "Фінал", BookText = "Повість закінчується безвихідною сваркою через грушу, яка засохла.", FilmText = "Серіал закінчується на тлі початку війни на Донбасі, що додає трагізму.", ImportanceLevel = "high", IsSpoiler = true },
                },
            });
        }

        // 17. Захар Беркут
        if (!await context.Works.AnyAsync(w => w.Title == "Захар Беркут"))
        {
            var author = new Author { Id = Guid.NewGuid(), FullName = "Іван Франко", Nationality = "Українець" };

            var book = new Book
            {
                Id = Guid.NewGuid(),
                Title = "Захар Беркут",
                Genre = "Історична повість",
                PublicationYear = 1883,
                CoverImageUrl = BerkutBookCoverUrl,
                Authors = new List<Author> { author },
            };

            var adaptation = new Adaptation
            {
                Id = Guid.NewGuid(),
                Title = "Захар Беркут (2019)",
                Type = "movie",
                ReleaseYear = 2019,
                Country = "Україна, США",
                PosterUrl = BerkutPosterUrl,
            };

            var work = new Work
            {
                Id = Guid.NewGuid(),
                Book = book,
                Adaptation = adaptation,
                Title = "Захар Беркут",
                Summary = "Екранізація історичної повісті Івана Франка.",
            };

            await context.Works.AddAsync(work);
            await context.Ratings.AddAsync(new Rating { Id = Guid.NewGuid(), WorkId = work.Id, BookRating = 9.4m, AdaptationRating = 8.1m, VotesCount = 1 });

            await context.DifferenceMaps.AddAsync(new DifferenceMap
            {
                Id = Guid.NewGuid(),
                WorkId = work.Id,
                Title = "Мапа розбіжностей: Захар Беркут",
                Differences = new List<Difference>
                {
                    new Difference { Id = Guid.NewGuid(), Title = "Динаміка битв", BookText = "Акцент на стратегії та мудрості громади Тухольщини.", FilmText = "Додано багато голлівудського екшену та бойових сцен для видовищності.", ImportanceLevel = "medium" },
                    new Difference { Id = Guid.NewGuid(), Title = "Персонаж Максима", BookText = "Максим показаний як взірець мужності та вірності громаді.", FilmText = "Образ Максима більш орієнтований на сучасного героя бойовиків.", ImportanceLevel = "low" },
                },
            });
        }

        // 18. Зелена миля (The Green Mile)
        if (!await context.Works.AnyAsync(w => w.Title == "Зелена миля"))
        {
            var author = new Author { Id = Guid.NewGuid(), FullName = "Стівен Кінґ", Nationality = "Американець" };

            var book = new Book
            {
                Id = Guid.NewGuid(),
                Title = "Зелена миля",
                Genre = "Драма",
                PublicationYear = 1996,
                CoverImageUrl = GreenMileBookCoverUrl,
                Authors = new List<Author> { author },
            };

            var adaptation = new Adaptation
            {
                Id = Guid.NewGuid(),
                Title = "Зелена миля (1999)",
                Type = "movie",
                ReleaseYear = 1999,
                Country = "США",
                PosterUrl = GreenMilePosterUrl,
            };

            var work = new Work
            {
                Id = Guid.NewGuid(),
                Book = book,
                Adaptation = adaptation,
                Title = "Зелена миля",
                Summary = "Культова драма за романом Стівена Кінґа.",
            };

            await context.Works.AddAsync(work);
            await context.Ratings.AddAsync(new Rating { Id = Guid.NewGuid(), WorkId = work.Id, BookRating = 9.8m, AdaptationRating = 9.9m, VotesCount = 1 });

            await context.DifferenceMaps.AddAsync(new DifferenceMap
            {
                Id = Guid.NewGuid(),
                WorkId = work.Id,
                Title = "Мапа розбіжностей: Зелена миля",
                Differences = new List<Difference>
                {
                    new Difference { Id = Guid.NewGuid(), Title = "Час дії", BookText = "Старий Пол Еджкомб розповідає історію у 1990-х роках, перебуваючи в будинку престарілих.", FilmText = "Рамкова історія значно скорочена, основна дія відбувається у 1930-х.", ImportanceLevel = "medium" },
                    new Difference { Id = Guid.NewGuid(), Title = "Доля персонажів", BookText = "Книга детально описує смерть дружини Пола та його довге життя як прокляття.", FilmText = "Фільм завершується на емоційній ноті спогадів, не заглиблюючись у подальші трагедії Пола.", ImportanceLevel = "medium", IsSpoiler = true },
                },
            });
        }

        // 19. Голодні ігри (The Hunger Games)
        if (!await context.Works.AnyAsync(w => w.Title == "Голодні ігри"))
        {
            var author = new Author { Id = Guid.NewGuid(), FullName = "Сюзанна Коллінз", Nationality = "Американка" };

            var book = new Book
            {
                Id = Guid.NewGuid(),
                Title = "Голодні ігри",
                Genre = "Антиутопія",
                PublicationYear = 2008,
                CoverImageUrl = HungerGamesBookCoverUrl,
                Authors = new List<Author> { author },
            };

            var adaptation = new Adaptation
            {
                Id = Guid.NewGuid(),
                Title = "Голодні ігри (2012)",
                Type = "movie",
                ReleaseYear = 2012,
                Country = "США",
                PosterUrl = HungerGamesPosterUrl,
            };

            var work = new Work
            {
                Id = Guid.NewGuid(),
                Book = book,
                Adaptation = adaptation,
                Title = "Голодні ігри",
                Summary = "Перша частина популярної антиутопічної трилогії.",
            };

            await context.Works.AddAsync(work);
            await context.Ratings.AddAsync(new Rating { Id = Guid.NewGuid(), WorkId = work.Id, BookRating = 9.1m, AdaptationRating = 8.6m, VotesCount = 1 });

            await context.DifferenceMaps.AddAsync(new DifferenceMap
            {
                Id = Guid.NewGuid(),
                WorkId = work.Id,
                Title = "Мапа розбіжностей: Голодні ігри",
                Differences = new List<Difference>
                {
                    new Difference { Id = Guid.NewGuid(), Title = "Точка зору", BookText = "Оповідь ведеться від першої особи Китнісс, ми знаємо всі її внутрішні страхи.", FilmText = "Фільм показує події ззовні, додаючи сцени з розпорядником ігор Сенекою Крейном.", ImportanceLevel = "high" },
                    new Difference { Id = Guid.NewGuid(), Title = "Брошка Сойки-пересмішниці", BookText = "Мадж (донька мера) дарує Китнісс брошку як знак дружби.", FilmText = "Китнісс купує брошку на ринку і дарує її Прім як оберіг.", ImportanceLevel = "medium" },
                },
            });
        }

        // 20. Острів проклятих (Shutter Island)
        if (!await context.Works.AnyAsync(w => w.Title == "Острів проклятих"))
        {
            var author = new Author { Id = Guid.NewGuid(), FullName = "Денніс Лігейн", Nationality = "Американець" };

            var book = new Book
            {
                Id = Guid.NewGuid(),
                Title = "Острів проклятих",
                Genre = "Психологічний трилер",
                PublicationYear = 2003,
                CoverImageUrl = ShutterIslandBookCoverUrl,
                Authors = new List<Author> { author },
            };

            var adaptation = new Adaptation
            {
                Id = Guid.NewGuid(),
                Title = "Острів проклятих (2010)",
                Type = "movie",
                ReleaseYear = 2010,
                Country = "США",
                PosterUrl = ShutterIslandPosterUrl,
            };

            var work = new Work
            {
                Id = Guid.NewGuid(),
                Book = book,
                Adaptation = adaptation,
                Title = "Острів проклятих",
                Summary = "Напружений трилер Мартіна Скорсезе за романом Денніса Лігейна.",
            };

            await context.Works.AddAsync(work);
            await context.Ratings.AddAsync(new Rating { Id = Guid.NewGuid(), WorkId = work.Id, BookRating = 9.3m, AdaptationRating = 9.2m, VotesCount = 1 });

            await context.DifferenceMaps.AddAsync(new DifferenceMap
            {
                Id = Guid.NewGuid(),
                WorkId = work.Id,
                Title = "Мапа розбіжностей: Острів проклятих",
                Differences = new List<Difference>
                {
                    new Difference { Id = Guid.NewGuid(), Title = "Фінальна фраза", BookText = "Книга закінчується без легендарної фрази про монстра чи людину.", FilmText = "Тедді каже: 'Що гірше: жити монстром чи померти людиною?', натякаючи на свій вибір.", ImportanceLevel = "high", IsSpoiler = true },
                    new Difference { Id = Guid.NewGuid(), Title = "Галюцинації", BookText = "Внутрішній стан Тедді описаний через довгі потоки свідомості.", FilmText = "Візуалізовані сюрреалістичні сни та попіл, що летить у кімнаті.", ImportanceLevel = "medium" },
                },
            });
        }

        // 21. Форест Гамп (Forrest Gump)
        if (!await context.Works.AnyAsync(w => w.Title == "Форест Гамп"))
        {
            var author = new Author { Id = Guid.NewGuid(), FullName = "Вінстон Ґрум", Nationality = "Американець" };

            var book = new Book
            {
                Id = Guid.NewGuid(),
                Title = "Форест Гамп",
                Genre = "Драма",
                PublicationYear = 1986,
                CoverImageUrl = ForrestGumpBookCoverUrl,
                Authors = new List<Author> { author },
            };

            var adaptation = new Adaptation
            {
                Id = Guid.NewGuid(),
                Title = "Форест Гамп (1994)",
                Type = "movie",
                ReleaseYear = 1994,
                Country = "США",
                PosterUrl = ForrestGumpPosterUrl,
            };

            var work = new Work
            {
                Id = Guid.NewGuid(),
                Book = book,
                Adaptation = adaptation,
                Title = "Форест Гамп",
                Summary = "Неймовірна історія життя Фореста Гампа.",
            };

            await context.Works.AddAsync(work);
            await context.Ratings.AddAsync(new Rating { Id = Guid.NewGuid(), WorkId = work.Id, BookRating = 8.5m, AdaptationRating = 9.6m, VotesCount = 1 });

            await context.DifferenceMaps.AddAsync(new DifferenceMap
            {
                Id = Guid.NewGuid(),
                WorkId = work.Id,
                Title = "Мапа розбіжностей: Форест Гамп",
                Differences = new List<Difference>
                {
                    new Difference { Id = Guid.NewGuid(), Title = "Характер героя", BookText = "Форест у книзі цинічніший, він навіть літає у космос з мавпою і стає рестлером.", FilmText = "Форест у фільмі — втілення чистоти та доброти, його історія більш лірична.", ImportanceLevel = "high" },
                    new Difference { Id = Guid.NewGuid(), Title = "Філософська фраза", BookText = "Книга починається фразою: 'Бути ідіотом — це не цукерка'.", FilmText = "Фільм подарував нам фразу: 'Життя — як коробка цукерок'.", ImportanceLevel = "medium" },
                },
            });
        }

        await context.SaveChangesAsync();
    }
}
