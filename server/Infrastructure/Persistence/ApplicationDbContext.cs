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

            entity.ToTable(t => t.HasCheckConstraint("CK_User_Role", "role IN ('user', 'admin', 'moderator')"));
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
            entity.ToTable(t => t.HasCheckConstraint("CK_Rating_Book", "book_rating >= 0 AND book_rating <= 10"));
            entity.ToTable(t => t.HasCheckConstraint("CK_Rating_Adaptation", "adaptation_rating >= 0 AND adaptation_rating <= 10"));
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
