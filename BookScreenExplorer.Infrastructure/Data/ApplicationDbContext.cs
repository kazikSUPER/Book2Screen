using BookScreenExplorer.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace BookScreenExplorer.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Author> Authors => Set<Author>();
    public DbSet<Book> Books => Set<Book>();
    public DbSet<Adaptation> Adaptations => Set<Adaptation>();
    public DbSet<Actor> Actors => Set<Actor>();
    public DbSet<Work> Works => Set<Work>();
    public DbSet<BookAuthor> BookAuthors => Set<BookAuthor>();
    public DbSet<AdaptationActor> AdaptationActors => Set<AdaptationActor>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<Vote> Votes => Set<Vote>();
    public DbSet<Rating> Ratings => Set<Rating>();
    public DbSet<PlotEvent> PlotEvents => Set<PlotEvent>();
    public DbSet<DifferenceMap> DifferenceMaps => Set<DifferenceMap>();
    public DbSet<Difference> Differences => Set<Difference>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureUsers(modelBuilder);
        ConfigureAuthors(modelBuilder);
        ConfigureBooks(modelBuilder);
        ConfigureAdaptations(modelBuilder);
        ConfigureActors(modelBuilder);
        ConfigureWorks(modelBuilder);
        ConfigureBookAuthors(modelBuilder);
        ConfigureAdaptationActors(modelBuilder);
        ConfigureReviews(modelBuilder);
        ConfigureVotes(modelBuilder);
        ConfigureRatings(modelBuilder);
        ConfigurePlotEvents(modelBuilder);
        ConfigureDifferenceMaps(modelBuilder);
        ConfigureDifferences(modelBuilder);
    }

    private static void ConfigureUsers(ModelBuilder modelBuilder)
    {
        var e = modelBuilder.Entity<User>();
        e.ToTable("users");
        e.HasKey(x => x.Id);
        e.Property(x => x.Username).HasMaxLength(50).IsRequired();
        e.Property(x => x.Email).HasMaxLength(255).IsRequired();
        e.Property(x => x.PasswordHash).IsRequired();
        e.Property(x => x.Role).HasMaxLength(20).IsRequired();
        e.Property(x => x.RegistrationDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
        e.Property(x => x.IsActive).HasDefaultValue(true);
        e.HasIndex(x => x.Username).IsUnique();
        e.HasIndex(x => x.Email).IsUnique();
        e.ToTable(t => t.HasCheckConstraint("CK_users_role", "role IN ('user', 'admin', 'moderator')"));
    }

    private static void ConfigureAuthors(ModelBuilder modelBuilder)
    {
        var e = modelBuilder.Entity<Author>();
        e.ToTable("authors");
        e.HasKey(x => x.Id);
        e.Property(x => x.FullName).HasMaxLength(150).IsRequired();
        e.Property(x => x.Nationality).HasMaxLength(100);
    }

    private static void ConfigureBooks(ModelBuilder modelBuilder)
    {
        var e = modelBuilder.Entity<Book>();
        e.ToTable("books");
        e.HasKey(x => x.Id);
        e.Property(x => x.Title).HasMaxLength(255).IsRequired();
        e.Property(x => x.Genre).HasMaxLength(100);
        e.Property(x => x.Language).HasMaxLength(50);
        e.Property(x => x.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        e.Property(x => x.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        e.ToTable(t => t.HasCheckConstraint("CK_books_publication_year", "publication_year IS NULL OR publication_year > 0"));
    }

    private static void ConfigureAdaptations(ModelBuilder modelBuilder)
    {
        var e = modelBuilder.Entity<Adaptation>();
        e.ToTable("adaptations");
        e.HasKey(x => x.Id);
        e.Property(x => x.Title).HasMaxLength(255).IsRequired();
        e.Property(x => x.Type).HasMaxLength(20).IsRequired();
        e.Property(x => x.Studio).HasMaxLength(150);
        e.Property(x => x.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        e.Property(x => x.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        e.ToTable(t =>
        {
            t.HasCheckConstraint("CK_adaptations_type", "type IN ('movie', 'series')");
            t.HasCheckConstraint("CK_adaptations_release_year", "release_year IS NULL OR release_year > 0");
            t.HasCheckConstraint("CK_adaptations_duration_minutes", "duration_minutes IS NULL OR duration_minutes > 0");
        });
    }

    private static void ConfigureActors(ModelBuilder modelBuilder)
    {
        var e = modelBuilder.Entity<Actor>();
        e.ToTable("actors");
        e.HasKey(x => x.Id);
        e.Property(x => x.FullName).HasMaxLength(150).IsRequired();
        e.Property(x => x.Nationality).HasMaxLength(100);
    }

    private static void ConfigureWorks(ModelBuilder modelBuilder)
    {
        var e = modelBuilder.Entity<Work>();
        e.ToTable("works");
        e.HasKey(x => x.Id);
        e.Property(x => x.Title).HasMaxLength(255).IsRequired();
        e.Property(x => x.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        e.Property(x => x.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        e.HasIndex(x => x.BookId).IsUnique();
        e.HasIndex(x => x.AdaptationId).IsUnique();

        e.HasOne(x => x.Book)
            .WithOne(x => x.Work)
            .HasForeignKey<Work>(x => x.BookId)
            .OnDelete(DeleteBehavior.Cascade);

        e.HasOne(x => x.Adaptation)
            .WithOne(x => x.Work)
            .HasForeignKey<Work>(x => x.AdaptationId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigureBookAuthors(ModelBuilder modelBuilder)
    {
        var e = modelBuilder.Entity<BookAuthor>();
        e.ToTable("book_authors");
        e.HasKey(x => x.Id);
        e.HasIndex(x => new { x.BookId, x.AuthorId }).IsUnique();
        e.HasOne(x => x.Book).WithMany(x => x.BookAuthors).HasForeignKey(x => x.BookId).OnDelete(DeleteBehavior.Cascade);
        e.HasOne(x => x.Author).WithMany(x => x.BookAuthors).HasForeignKey(x => x.AuthorId).OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigureAdaptationActors(ModelBuilder modelBuilder)
    {
        var e = modelBuilder.Entity<AdaptationActor>();
        e.ToTable("adaptation_actors");
        e.HasKey(x => x.Id);
        e.Property(x => x.RoleName).HasMaxLength(150);
        e.HasIndex(x => new { x.AdaptationId, x.ActorId, x.RoleName }).IsUnique();
        e.HasOne(x => x.Adaptation).WithMany(x => x.AdaptationActors).HasForeignKey(x => x.AdaptationId).OnDelete(DeleteBehavior.Cascade);
        e.HasOne(x => x.Actor).WithMany(x => x.AdaptationActors).HasForeignKey(x => x.ActorId).OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigureReviews(ModelBuilder modelBuilder)
    {
        var e = modelBuilder.Entity<Review>();
        e.ToTable("reviews");
        e.HasKey(x => x.Id);
        e.Property(x => x.TargetType).HasMaxLength(20).IsRequired();
        e.Property(x => x.Text).IsRequired();
        e.Property(x => x.LikesCount).HasDefaultValue(0);
        e.Property(x => x.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        e.HasOne(x => x.User).WithMany(x => x.Reviews).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.SetNull);
        e.HasOne(x => x.Work).WithMany(x => x.Reviews).HasForeignKey(x => x.WorkId).OnDelete(DeleteBehavior.Cascade);
        e.ToTable(t =>
        {
            t.HasCheckConstraint("CK_reviews_target_type", "target_type IN ('book', 'adaptation', 'comparison')");
            t.HasCheckConstraint("CK_reviews_likes_count", "likes_count >= 0");
        });
    }

    private static void ConfigureVotes(ModelBuilder modelBuilder)
    {
        var e = modelBuilder.Entity<Vote>();
        e.ToTable("votes");
        e.HasKey(x => x.Id);
        e.Property(x => x.SelectedOption).HasMaxLength(20).IsRequired();
        e.Property(x => x.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        e.HasIndex(x => new { x.UserId, x.WorkId }).IsUnique();
        e.HasOne(x => x.User).WithMany(x => x.Votes).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.SetNull);
        e.HasOne(x => x.Work).WithMany(x => x.Votes).HasForeignKey(x => x.WorkId).OnDelete(DeleteBehavior.Cascade);
        e.ToTable(t => t.HasCheckConstraint("CK_votes_selected_option", "selected_option IN ('book', 'adaptation')"));
    }

    private static void ConfigureRatings(ModelBuilder modelBuilder)
    {
        var e = modelBuilder.Entity<Rating>();
        e.ToTable("ratings");
        e.HasKey(x => x.Id);
        e.HasIndex(x => x.WorkId).IsUnique();
        e.Property(x => x.BookRating).HasColumnType("numeric(3,2)");
        e.Property(x => x.AdaptationRating).HasColumnType("numeric(3,2)");
        e.Property(x => x.VotesCount).HasDefaultValue(0);
        e.Property(x => x.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        e.HasOne(x => x.Work).WithOne(x => x.Rating).HasForeignKey<Rating>(x => x.WorkId).OnDelete(DeleteBehavior.Cascade);
        e.ToTable(t =>
        {
            t.HasCheckConstraint("CK_ratings_book_rating", "book_rating IS NULL OR (book_rating >= 0 AND book_rating <= 10)");
            t.HasCheckConstraint("CK_ratings_adaptation_rating", "adaptation_rating IS NULL OR (adaptation_rating >= 0 AND adaptation_rating <= 10)");
            t.HasCheckConstraint("CK_ratings_votes_count", "votes_count >= 0");
        });
    }

    private static void ConfigurePlotEvents(ModelBuilder modelBuilder)
    {
        var e = modelBuilder.Entity<PlotEvent>();
        e.ToTable("plot_events");
        e.HasKey(x => x.Id);
        e.Property(x => x.SourceType).HasMaxLength(20).IsRequired();
        e.Property(x => x.Title).HasMaxLength(255).IsRequired();
        e.Property(x => x.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        e.HasOne(x => x.Work).WithMany(x => x.PlotEvents).HasForeignKey(x => x.WorkId).OnDelete(DeleteBehavior.Cascade);
        e.ToTable(t =>
        {
            t.HasCheckConstraint("CK_plot_events_source_type", "source_type IN ('book', 'adaptation')");
            t.HasCheckConstraint("CK_plot_events_sequence_number", "sequence_number > 0");
            t.HasCheckConstraint("CK_plot_events_episode_number", "episode_number IS NULL OR episode_number > 0");
            t.HasCheckConstraint("CK_plot_events_season_number", "season_number IS NULL OR season_number > 0");
        });
    }

    private static void ConfigureDifferenceMaps(ModelBuilder modelBuilder)
    {
        var e = modelBuilder.Entity<DifferenceMap>();
        e.ToTable("difference_maps");
        e.HasKey(x => x.Id);
        e.Property(x => x.Title).HasMaxLength(255).IsRequired();
        e.Property(x => x.Version).HasDefaultValue(1);
        e.Property(x => x.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        e.Property(x => x.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        e.HasIndex(x => x.WorkId).IsUnique();
        e.HasOne(x => x.Work).WithOne(x => x.DifferenceMap).HasForeignKey<DifferenceMap>(x => x.WorkId).OnDelete(DeleteBehavior.Cascade);
        e.ToTable(t => t.HasCheckConstraint("CK_difference_maps_version", "version > 0"));
    }

    private static void ConfigureDifferences(ModelBuilder modelBuilder)
    {
        var e = modelBuilder.Entity<Difference>();
        e.ToTable("differences");
        e.HasKey(x => x.Id);
        e.Property(x => x.DifferenceType).HasMaxLength(20).IsRequired();
        e.Property(x => x.Description).IsRequired();
        e.Property(x => x.ImportanceLevel).HasMaxLength(20).IsRequired();
        e.Property(x => x.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        e.HasOne(x => x.Map).WithMany(x => x.Differences).HasForeignKey(x => x.MapId).OnDelete(DeleteBehavior.Cascade);
        e.HasOne(x => x.BookEvent).WithMany(x => x.BookDifferences).HasForeignKey(x => x.BookEventId).OnDelete(DeleteBehavior.SetNull);
        e.HasOne(x => x.AdaptationEvent).WithMany(x => x.AdaptationDifferences).HasForeignKey(x => x.AdaptationEventId).OnDelete(DeleteBehavior.SetNull);
        e.ToTable(t =>
        {
            t.HasCheckConstraint("CK_differences_difference_type", "difference_type IN ('changed', 'added', 'removed')");
            t.HasCheckConstraint("CK_differences_importance_level", "importance_level IN ('low', 'medium', 'high')");
            t.HasCheckConstraint("CK_differences_events", "book_event_id IS NOT NULL OR adaptation_event_id IS NOT NULL");
        });
    }
}
