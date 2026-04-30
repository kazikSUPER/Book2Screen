using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookScreenExplorer.Infrastructure.Migrations
{
    public partial class AddSearchAndFilterIndexes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS idx_books_title ON ""Books"" (""Title"");
                CREATE INDEX IF NOT EXISTS idx_books_genre ON ""Books"" (""Genre"");
                CREATE INDEX IF NOT EXISTS idx_books_publication_year ON ""Books"" (""PublicationYear"");
                CREATE INDEX IF NOT EXISTS idx_books_language ON ""Books"" (""Language"");

                CREATE INDEX IF NOT EXISTS idx_adaptations_title ON ""Adaptations"" (""Title"");
                CREATE INDEX IF NOT EXISTS idx_adaptations_type ON ""Adaptations"" (""Type"");
                CREATE INDEX IF NOT EXISTS idx_adaptations_release_year ON ""Adaptations"" (""ReleaseYear"");
                CREATE INDEX IF NOT EXISTS idx_adaptations_studio ON ""Adaptations"" (""Studio"");

                CREATE INDEX IF NOT EXISTS idx_authors_full_name ON ""Authors"" (""FullName"");
                CREATE INDEX IF NOT EXISTS idx_actors_full_name ON ""Actors"" (""FullName"");

                CREATE INDEX IF NOT EXISTS idx_reviews_work_id ON ""Reviews"" (""WorkId"");
                CREATE INDEX IF NOT EXISTS idx_reviews_user_id ON ""Reviews"" (""UserId"");
                CREATE INDEX IF NOT EXISTS idx_reviews_target_type ON ""Reviews"" (""TargetType"");
                CREATE INDEX IF NOT EXISTS idx_reviews_created_at ON ""Reviews"" (""CreatedAt"");

                CREATE INDEX IF NOT EXISTS idx_ratings_book_rating ON ""Ratings"" (""BookRating"");
                CREATE INDEX IF NOT EXISTS idx_ratings_adaptation_rating ON ""Ratings"" (""AdaptationRating"");
                CREATE INDEX IF NOT EXISTS idx_ratings_votes_count ON ""Ratings"" (""VotesCount"");

                CREATE INDEX IF NOT EXISTS idx_plot_events_work_id ON ""PlotEvents"" (""WorkId"");
                CREATE INDEX IF NOT EXISTS idx_plot_events_source_type ON ""PlotEvents"" (""SourceType"");
                CREATE INDEX IF NOT EXISTS idx_plot_events_sequence_number ON ""PlotEvents"" (""SequenceNumber"");

                CREATE INDEX IF NOT EXISTS idx_differences_map_id ON ""Differences"" (""MapId"");
                CREATE INDEX IF NOT EXISTS idx_differences_difference_type ON ""Differences"" (""DifferenceType"");
                CREATE INDEX IF NOT EXISTS idx_differences_importance_level ON ""Differences"" (""ImportanceLevel"");
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DROP INDEX IF EXISTS idx_books_title;
                DROP INDEX IF EXISTS idx_books_genre;
                DROP INDEX IF EXISTS idx_books_publication_year;
                DROP INDEX IF EXISTS idx_books_language;

                DROP INDEX IF EXISTS idx_adaptations_title;
                DROP INDEX IF EXISTS idx_adaptations_type;
                DROP INDEX IF EXISTS idx_adaptations_release_year;
                DROP INDEX IF EXISTS idx_adaptations_studio;

                DROP INDEX IF EXISTS idx_authors_full_name;
                DROP INDEX IF EXISTS idx_actors_full_name;

                DROP INDEX IF EXISTS idx_reviews_work_id;
                DROP INDEX IF EXISTS idx_reviews_user_id;
                DROP INDEX IF EXISTS idx_reviews_target_type;
                DROP INDEX IF EXISTS idx_reviews_created_at;

                DROP INDEX IF EXISTS idx_ratings_book_rating;
                DROP INDEX IF EXISTS idx_ratings_adaptation_rating;
                DROP INDEX IF EXISTS idx_ratings_votes_count;

                DROP INDEX IF EXISTS idx_plot_events_work_id;
                DROP INDEX IF EXISTS idx_plot_events_source_type;
                DROP INDEX IF EXISTS idx_plot_events_sequence_number;

                DROP INDEX IF EXISTS idx_differences_map_id;
                DROP INDEX IF EXISTS idx_differences_difference_type;
                DROP INDEX IF EXISTS idx_differences_importance_level;
            ");
        }
    }
}
