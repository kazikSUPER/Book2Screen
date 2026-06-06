using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Book2Screen.Migrations
{
    /// <inheritdoc />
    public partial class FixFavoriteUniqueness : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Видаляємо старі поодинокі індекси, які тепер покриваються складеними індексами з AddPerformanceIndexes
            migrationBuilder.DropIndex(
                name: "IX_Votes_WorkId",
                table: "Votes");

            migrationBuilder.DropIndex(
                name: "IX_Reviews_UserId",
                table: "Reviews");

            migrationBuilder.DropIndex(
                name: "IX_Reviews_WorkId",
                table: "Reviews");

            migrationBuilder.DropIndex(
                name: "IX_PlotEvent_WorkId",
                table: "PlotEvent");

            migrationBuilder.DropIndex(
                name: "IX_Favorites_UserId_WorkId",
                table: "Favorites");

            migrationBuilder.CreateIndex(
                name: "IX_PasswordResetTokens_Code",
                table: "PasswordResetTokens",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Favorites_UserId_WorkId_Kind",
                table: "Favorites",
                columns: new[] { "UserId", "WorkId", "Kind" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PasswordResetTokens_Code",
                table: "PasswordResetTokens");

            migrationBuilder.DropIndex(
                name: "IX_Favorites_UserId_WorkId_Kind",
                table: "Favorites");

            migrationBuilder.CreateIndex(
                name: "IX_Votes_WorkId",
                table: "Votes",
                column: "WorkId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_UserId",
                table: "Reviews",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_WorkId",
                table: "Reviews",
                column: "WorkId");

            migrationBuilder.CreateIndex(
                name: "IX_PlotEvent_WorkId",
                table: "PlotEvent",
                column: "WorkId");

            migrationBuilder.CreateIndex(
                name: "IX_Favorites_UserId_WorkId",
                table: "Favorites",
                columns: new[] { "UserId", "WorkId" },
                unique: true);
        }
    }
}
