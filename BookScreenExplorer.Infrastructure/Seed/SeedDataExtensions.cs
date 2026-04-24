using BookScreenExplorer.Infrastructure.Data;
using BookScreenExplorer.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace BookScreenExplorer.Infrastructure.Seed;

public static class SeedDataExtensions
{
    public static async Task SeedAsync(this ApplicationDbContext context, CancellationToken cancellationToken = default)
    {
        if (await context.Users.AnyAsync(cancellationToken))
            return;

        var user1 = new User
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Username = "admin",
            Email = "admin@example.com",
            PasswordHash = "seeded-hash-1",
            Role = "admin",
            RegistrationDate = DateTime.UtcNow,
            IsActive = true
        };

        var user2 = new User
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            Username = "reader_anna",
            Email = "anna@example.com",
            PasswordHash = "seeded-hash-2",
            Role = "user",
            RegistrationDate = DateTime.UtcNow,
            IsActive = true
        };

        var author = new Author
        {
            Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            FullName = "Frank Herbert",
            Nationality = "American",
            Biography = "Author of the Dune saga."
        };

        var actor = new Actor
        {
            Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
            FullName = "Timothée Chalamet",
            Nationality = "American"
        };

        var book = new Book
        {
            Id = Guid.Parse("55555555-5555-5555-5555-555555555555"),
            Title = "Dune",
            Description = "Science fiction novel.",
            Genre = "Science Fiction",
            PublicationYear = 1965,
            Language = "English",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var adaptation = new Adaptation
        {
            Id = Guid.Parse("66666666-6666-6666-6666-666666666666"),
            Title = "Dune",
            Type = "movie",
            Description = "Film adaptation directed by Denis Villeneuve.",
            ReleaseYear = 2021,
            DurationMinutes = 155,
            Studio = "Legendary Pictures",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var work = new Work
        {
            Id = Guid.Parse("77777777-7777-7777-7777-777777777777"),
            BookId = book.Id,
            AdaptationId = adaptation.Id,
            Title = "Dune: Book vs Adaptation",
            Summary = "Comparison between the original novel and the movie adaptation.",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var rating = new Rating
        {
            Id = Guid.Parse("88888888-8888-8888-8888-888888888888"),
            WorkId = work.Id,
            BookRating = 9.60m,
            AdaptationRating = 8.80m,
            VotesCount = 2,
            UpdatedAt = DateTime.UtcNow
        };

        var review = new Review
        {
            Id = Guid.Parse("99999999-9999-9999-9999-999999999999"),
            UserId = user2.Id,
            WorkId = work.Id,
            TargetType = "comparison",
            Text = "The adaptation is visually stunning, but the book is deeper.",
            LikesCount = 3,
            CreatedAt = DateTime.UtcNow
        };

        var vote = new Vote
        {
            Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            UserId = user2.Id,
            WorkId = work.Id,
            SelectedOption = "book",
            CreatedAt = DateTime.UtcNow
        };

        var differenceMap = new DifferenceMap
        {
            Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
            WorkId = work.Id,
            Title = "Main differences",
            Version = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var bookEvent = new PlotEvent
        {
            Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
            WorkId = work.Id,
            SourceType = "book",
            Title = "Paul meets Reverend Mother",
            Description = "Book version of the Gom Jabbar test.",
            SequenceNumber = 1,
            CreatedAt = DateTime.UtcNow
        };

        var adaptationEvent = new PlotEvent
        {
            Id = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"),
            WorkId = work.Id,
            SourceType = "adaptation",
            Title = "Paul meets Reverend Mother",
            Description = "Movie version of the Gom Jabbar test.",
            SequenceNumber = 1,
            CreatedAt = DateTime.UtcNow
        };

        var difference = new Difference
        {
            Id = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"),
            MapId = differenceMap.Id,
            BookEventId = bookEvent.Id,
            AdaptationEventId = adaptationEvent.Id,
            DifferenceType = "changed",
            Description = "The movie compresses the internal monologue and symbolism.",
            ImportanceLevel = "medium",
            CreatedAt = DateTime.UtcNow
        };

        var bookAuthor = new BookAuthor
        {
            Id = Guid.Parse("f1f1f1f1-f1f1-f1f1-f1f1-f1f1f1f1f1f1"),
            BookId = book.Id,
            AuthorId = author.Id
        };

        var adaptationActor = new AdaptationActor
        {
            Id = Guid.Parse("f2f2f2f2-f2f2-f2f2-f2f2-f2f2f2f2f2f2"),
            AdaptationId = adaptation.Id,
            ActorId = actor.Id,
            RoleName = "Paul Atreides"
        };

        await context.Users.AddRangeAsync(user1, user2, cancellationToken);
        await context.Authors.AddAsync(author, cancellationToken);
        await context.Actors.AddAsync(actor, cancellationToken);
        await context.Books.AddAsync(book, cancellationToken);
        await context.Adaptations.AddAsync(adaptation, cancellationToken);
        await context.Works.AddAsync(work, cancellationToken);
        await context.BookAuthors.AddAsync(bookAuthor, cancellationToken);
        await context.AdaptationActors.AddAsync(adaptationActor, cancellationToken);
        await context.Ratings.AddAsync(rating, cancellationToken);
        await context.Reviews.AddAsync(review, cancellationToken);
        await context.Votes.AddAsync(vote, cancellationToken);
        await context.DifferenceMaps.AddAsync(differenceMap, cancellationToken);
        await context.PlotEvents.AddRangeAsync(bookEvent, adaptationEvent, cancellationToken);
        await context.Differences.AddAsync(difference, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
    }
}
