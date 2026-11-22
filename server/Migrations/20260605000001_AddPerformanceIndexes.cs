// <copyright file="20260605000001_AddPerformanceIndexes.cs" company="Team 17">
// Copyright (c) Team 17. All rights reserved.
// </copyright>

namespace Book2Screen.Migrations
{
    using Microsoft.EntityFrameworkCore.Migrations;

    /// <inheritdoc />
    public partial class AddPerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ---------------------------------------------------------------
            // Reviews
            // ---------------------------------------------------------------

            // GET /api/v1/reviews/work/{workId}  — фільтрація відгуків за твором
            // + сортування за датою (найновіші першими)
            // Покриває: WHERE WorkId = ? ORDER BY CreatedAt DESC
            migrationBuilder.CreateIndex(
                name: "IX_Reviews_WorkId_CreatedAt",
                table: "Reviews",
                columns: new[] { "WorkId", "CreatedAt" });

            // GET /api/v1/users/me/reviews  — всі відгуки поточного користувача
            // + сортування за датою
            // Покриває: WHERE UserId = ? ORDER BY CreatedAt DESC
            migrationBuilder.CreateIndex(
                name: "IX_Reviews_UserId_CreatedAt",
                table: "Reviews",
                columns: new[] { "UserId", "CreatedAt" });

            // ---------------------------------------------------------------
            // Reports
            // ---------------------------------------------------------------

            // GET /api/v1/admin/reports  — адмін отримує скарги, найчастіше
            // фільтрує за Status = 'Pending' (черга модерації)
            // Покриває: WHERE Status = 'Pending' ORDER BY CreatedAt ASC
            migrationBuilder.CreateIndex(
                name: "IX_Reports_Status_CreatedAt",
                table: "Reports",
                columns: new[] { "Status", "CreatedAt" });

            // ---------------------------------------------------------------
            // Votes
            // ---------------------------------------------------------------

            // GET /api/v1/votes/{workId}  — статистика голосів за твором
            // Покриває: WHERE WorkId = ? GROUP BY SelectedOption
            migrationBuilder.CreateIndex(
                name: "IX_Votes_WorkId_SelectedOption",
                table: "Votes",
                columns: new[] { "WorkId", "SelectedOption" });

            // ---------------------------------------------------------------
            // Favorites
            // ---------------------------------------------------------------

            // GET /api/v1/favorites  — список обраного користувача
            // + сортування за датою додавання
            // Покриває: WHERE UserId = ? ORDER BY CreatedAt DESC
            migrationBuilder.CreateIndex(
                name: "IX_Favorites_UserId_CreatedAt",
                table: "Favorites",
                columns: new[] { "UserId", "CreatedAt" });

            // ---------------------------------------------------------------
            // PasswordResetTokens
            // ---------------------------------------------------------------

            // POST /api/v1/auth/verify-code та reset-password
            // Покриває: WHERE Email = ? AND IsUsed = false AND ExpiryTime > now()
            migrationBuilder.CreateIndex(
                name: "IX_PasswordResetTokens_Email_IsUsed",
                table: "PasswordResetTokens",
                columns: new[] { "Email", "IsUsed" });

            // ---------------------------------------------------------------
            // Works
            // ---------------------------------------------------------------

            // GET /api/v1/works/top?count=10  — топ за рейтингом адаптації
            // JOIN Works → Ratings ORDER BY AdaptationRating DESC LIMIT N
            // Покриває сортування на стороні Ratings
            migrationBuilder.CreateIndex(
                name: "IX_Ratings_AdaptationRating",
                table: "Ratings",
                column: "AdaptationRating");

            // ---------------------------------------------------------------
            // PlotEvent
            // ---------------------------------------------------------------

            // Карта відмінностей: вибір подій для конкретного Work
            // за типом джерела (book / adaptation)
            // Покриває: WHERE WorkId = ? AND SourceType = ?
            // ORDER BY SequenceNumber ASC
            migrationBuilder.CreateIndex(
                name: "IX_PlotEvent_WorkId_SourceType_SequenceNumber",
                table: "PlotEvent",
                columns: new[] { "WorkId", "SourceType", "SequenceNumber" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Reviews_WorkId_CreatedAt",
                table: "Reviews");

            migrationBuilder.DropIndex(
                name: "IX_Reviews_UserId_CreatedAt",
                table: "Reviews");

            migrationBuilder.DropIndex(
                name: "IX_Reports_Status_CreatedAt",
                table: "Reports");

            migrationBuilder.DropIndex(
                name: "IX_Votes_WorkId_SelectedOption",
                table: "Votes");

            migrationBuilder.DropIndex(
                name: "IX_Favorites_UserId_CreatedAt",
                table: "Favorites");

            migrationBuilder.DropIndex(
                name: "IX_PasswordResetTokens_Email_IsUsed",
                table: "PasswordResetTokens");

            migrationBuilder.DropIndex(
                name: "IX_Ratings_AdaptationRating",
                table: "Ratings");

            migrationBuilder.DropIndex(
                name: "IX_PlotEvent_WorkId_SourceType_SequenceNumber",
                table: "PlotEvent");
        }
    }
}
