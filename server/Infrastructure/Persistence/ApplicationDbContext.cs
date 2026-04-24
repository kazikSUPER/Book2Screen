namespace Book2Screen.Infrastructure.Persistence;

using System.Reflection;
using Book2Screen.Domain.Entities;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Контекст бази даних, що реалізує конфігурації для PostgreSQL (Supabase).
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => this.Set<User>();

    public DbSet<Book> Books => this.Set<Book>();

    public DbSet<Adaptation> Adaptations => this.Set<Adaptation>();

    public DbSet<Author> Authors => this.Set<Author>();

    public DbSet<Actor> Actors => this.Set<Actor>();

    public DbSet<Work> Works => this.Set<Work>();

    public DbSet<Review> Reviews => this.Set<Review>();

    public DbSet<Vote> Votes => this.Set<Vote>();

    public DbSet<Rating> Ratings => this.Set<Rating>();

    /// <inheritdoc/>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Автоматичне застосування конфігурацій з поточної збірки
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Налаштування User (CHECK constraints та Unique)
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Username).IsUnique();
            entity.HasIndex(u => u.Email).IsUnique();

            entity.ToTable(t => t.HasCheckConstraint("CK_User_Role", "\"Role\" IN ('user', 'admin', 'moderator')"));
        });

        // Налаштування Work (One-to-One зв'язки з унікальними FK)
        modelBuilder.Entity<Work>(entity =>
        {
            entity.HasIndex(w => w.BookId).IsUnique();
            entity.HasIndex(w => w.AdaptationId).IsUnique();

            entity.HasOne(w => w.Book)
                .WithOne(b => b.Work)
                .HasForeignKey<Work>(w => w.BookId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(w => w.Adaptation)
                .WithOne(a => a.Work)
                .HasForeignKey<Work>(w => w.AdaptationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Налаштування Review
        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasOne(r => r.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(r => r.Work)
                .WithMany(w => w.Reviews)
                .HasForeignKey(r => r.WorkId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Налаштування Vote
        modelBuilder.Entity<Vote>(entity =>
        {
            entity.HasIndex(v => new { v.UserId, v.WorkId }).IsUnique();

            entity.HasOne(v => v.User)
                .WithMany(u => u.Votes)
                .HasForeignKey(v => v.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(v => v.Work)
                .WithMany(w => w.Votes)
                .HasForeignKey(v => v.WorkId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.ToTable(t => t.HasCheckConstraint("CK_Vote_Option", "\"SelectedOption\" IN ('book', 'adaptation', 'movie')"));
        });

        // Багато-до-багатьох: Books <-> Authors
        modelBuilder.Entity<Book>()
            .HasMany(b => b.Authors)
            .WithMany(a => a.Books)
            .UsingEntity(j => j.ToTable("book_authors"));

        // Складний зв'язок M2M: Adaptation <-> Actor (черезе AdaptationActor)
        modelBuilder.Entity<AdaptationActor>(entity =>
        {
            entity.HasKey(aa => new { aa.AdaptationId, aa.ActorId, aa.RoleName });

            entity.HasOne(aa => aa.Adaptation)
                .WithMany(a => a.AdaptationActors)
                .HasForeignKey(aa => aa.AdaptationId);

            entity.HasOne(aa => aa.Actor)
                .WithMany(a => a.AdaptationActors)
                .HasForeignKey(aa => aa.ActorId);
        });

        // Налаштування Rating (1:1 з Work)
        modelBuilder.Entity<Rating>(entity =>
        {
            entity.HasIndex(r => r.WorkId).IsUnique();

            // Додаємо екрановані лапки та виправляємо назви на ті, що у вашому C# класі
            entity.ToTable(t => t.HasCheckConstraint("CK_Rating_Book", "\"BookRating\" >= 0 AND \"BookRating\" <= 10"));
            entity.ToTable(t => t.HasCheckConstraint("CK_Rating_Adaptation", "\"AdaptationRating\" >= 0 AND \"AdaptationRating\" <= 10"));
        });
    }

    /// <inheritdoc/>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = this.ChangeTracker.Entries()
            .Where(e => e.Entity is BaseEntity && (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entry in entries)
        {
            ((BaseEntity)entry.Entity).UpdatedAt = DateTime.UtcNow;
            if (entry.State == EntityState.Added)
            {
                ((BaseEntity)entry.Entity).CreatedAt = DateTime.UtcNow;
            }
        }
        return base.SaveChangesAsync(cancellationToken);
    }
}
