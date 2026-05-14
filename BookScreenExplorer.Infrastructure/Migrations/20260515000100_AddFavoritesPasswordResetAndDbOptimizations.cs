using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookScreenExplorer.Infrastructure.Migrations
{
    public partial class AddFavoritesPasswordResetAndDbOptimizations : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "password_reset_tokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    ExpiryTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsUsed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_password_reset_tokens", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "favorites",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_favorites", x => x.Id);

                    table.ForeignKey(
                        name: "FK_favorites_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);

                    table.ForeignKey(
                        name: "FK_favorites_works_WorkId",
                        column: x => x.WorkId,
                        principalTable: "works",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_favorites_UserId_WorkId",
                table: "favorites",
                columns: new[] { "UserId", "WorkId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_favorites_WorkId",
                table: "favorites",
                column: "WorkId");

            migrationBuilder.CreateIndex(
                name: "IX_books_Genre",
                table: "books",
                column: "Genre");

            migrationBuilder.CreateIndex(
                name: "IX_works_Title",
                table: "works",
                column: "Title");

            migrationBuilder.CreateIndex(
                name: "IX_adaptations_Country",
                table: "adaptations",
                column: "Country");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "favorites");

            migrationBuilder.DropTable(
                name: "password_reset_tokens");

            migrationBuilder.DropIndex(
                name: "IX_books_Genre",
                table: "books");

            migrationBuilder.DropIndex(
                name: "IX_works_Title",
                table: "works");

            migrationBuilder.DropIndex(
                name: "IX_adaptations_Country",
                table: "adaptations");
        }
    }
}
