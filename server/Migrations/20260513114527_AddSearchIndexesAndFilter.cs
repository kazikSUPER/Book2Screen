using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Book2Screen.Migrations
{
    /// <inheritdoc />
    public partial class AddSearchIndexesAndFilter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Difference_DifferenceMap_MapId",
                table: "Difference");

            migrationBuilder.DropForeignKey(
                name: "FK_Difference_PlotEvent_AdaptationEventId",
                table: "Difference");

            migrationBuilder.DropForeignKey(
                name: "FK_Difference_PlotEvent_BookEventId",
                table: "Difference");

            migrationBuilder.DropForeignKey(
                name: "FK_DifferenceMap_Works_WorkId",
                table: "DifferenceMap");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DifferenceMap",
                table: "DifferenceMap");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Difference",
                table: "Difference");

            migrationBuilder.RenameTable(
                name: "DifferenceMap",
                newName: "DifferenceMaps");

            migrationBuilder.RenameTable(
                name: "Difference",
                newName: "Differences");

            migrationBuilder.RenameIndex(
                name: "IX_DifferenceMap_WorkId",
                table: "DifferenceMaps",
                newName: "IX_DifferenceMaps_WorkId");

            migrationBuilder.RenameIndex(
                name: "IX_Difference_MapId",
                table: "Differences",
                newName: "IX_Differences_MapId");

            migrationBuilder.RenameIndex(
                name: "IX_Difference_BookEventId",
                table: "Differences",
                newName: "IX_Differences_BookEventId");

            migrationBuilder.RenameIndex(
                name: "IX_Difference_AdaptationEventId",
                table: "Differences",
                newName: "IX_Differences_AdaptationEventId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DifferenceMaps",
                table: "DifferenceMaps",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Differences",
                table: "Differences",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DifferenceMaps_Works_WorkId",
                table: "DifferenceMaps",
                column: "WorkId",
                principalTable: "Works",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Differences_DifferenceMaps_MapId",
                table: "Differences",
                column: "MapId",
                principalTable: "DifferenceMaps",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Differences_PlotEvent_AdaptationEventId",
                table: "Differences",
                column: "AdaptationEventId",
                principalTable: "PlotEvent",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Differences_PlotEvent_BookEventId",
                table: "Differences",
                column: "BookEventId",
                principalTable: "PlotEvent",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DifferenceMaps_Works_WorkId",
                table: "DifferenceMaps");

            migrationBuilder.DropForeignKey(
                name: "FK_Differences_DifferenceMaps_MapId",
                table: "Differences");

            migrationBuilder.DropForeignKey(
                name: "FK_Differences_PlotEvent_AdaptationEventId",
                table: "Differences");

            migrationBuilder.DropForeignKey(
                name: "FK_Differences_PlotEvent_BookEventId",
                table: "Differences");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Differences",
                table: "Differences");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DifferenceMaps",
                table: "DifferenceMaps");

            migrationBuilder.RenameTable(
                name: "Differences",
                newName: "Difference");

            migrationBuilder.RenameTable(
                name: "DifferenceMaps",
                newName: "DifferenceMap");

            migrationBuilder.RenameIndex(
                name: "IX_Differences_MapId",
                table: "Difference",
                newName: "IX_Difference_MapId");

            migrationBuilder.RenameIndex(
                name: "IX_Differences_BookEventId",
                table: "Difference",
                newName: "IX_Difference_BookEventId");

            migrationBuilder.RenameIndex(
                name: "IX_Differences_AdaptationEventId",
                table: "Difference",
                newName: "IX_Difference_AdaptationEventId");

            migrationBuilder.RenameIndex(
                name: "IX_DifferenceMaps_WorkId",
                table: "DifferenceMap",
                newName: "IX_DifferenceMap_WorkId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Difference",
                table: "Difference",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DifferenceMap",
                table: "DifferenceMap",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Difference_DifferenceMap_MapId",
                table: "Difference",
                column: "MapId",
                principalTable: "DifferenceMap",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Difference_PlotEvent_AdaptationEventId",
                table: "Difference",
                column: "AdaptationEventId",
                principalTable: "PlotEvent",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Difference_PlotEvent_BookEventId",
                table: "Difference",
                column: "BookEventId",
                principalTable: "PlotEvent",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DifferenceMap_Works_WorkId",
                table: "DifferenceMap",
                column: "WorkId",
                principalTable: "Works",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
