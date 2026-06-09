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

        await context.SaveChangesAsync();
    }
}
