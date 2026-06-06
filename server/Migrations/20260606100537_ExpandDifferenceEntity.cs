using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Book2Screen.Migrations
{
    /// <inheritdoc />
    public partial class ExpandDifferenceEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Differences",
                newName: "FilmText");

            migrationBuilder.AlterColumn<string>(
                name: "DifferenceType",
                table: "Differences",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AddColumn<string>(
                name: "BookText",
                table: "Differences",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsSpoiler",
                table: "Differences",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Differences",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BookText",
                table: "Differences");

            migrationBuilder.DropColumn(
                name: "IsSpoiler",
                table: "Differences");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "Differences");

            migrationBuilder.RenameColumn(
                name: "FilmText",
                table: "Differences",
                newName: "Description");

            migrationBuilder.AlterColumn<string>(
                name: "DifferenceType",
                table: "Differences",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldNullable: true);
        }
    }
}
