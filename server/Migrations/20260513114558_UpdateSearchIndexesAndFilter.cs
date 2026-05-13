// <copyright file="20260513114558_UpdateSearchIndexesAndFilter.cs" company="Team 17">
// Copyright (c) Team 17. All rights reserved.
// </copyright>

namespace Book2Screen.Migrations
{
    using Microsoft.EntityFrameworkCore.Migrations;

    #nullable disable

    /// <inheritdoc />
    public partial class UpdateSearchIndexesAndFilter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Works_Title",
                table: "Works",
                column: "Title");

            migrationBuilder.CreateIndex(
                name: "IX_Books_Genre",
                table: "Books",
                column: "Genre");

            migrationBuilder.CreateIndex(
                name: "IX_Adaptations_Country",
                table: "Adaptations",
                column: "Country");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Works_Title",
                table: "Works");

            migrationBuilder.DropIndex(
                name: "IX_Books_Genre",
                table: "Books");

            migrationBuilder.DropIndex(
                name: "IX_Adaptations_Country",
                table: "Adaptations");
        }
    }
}
