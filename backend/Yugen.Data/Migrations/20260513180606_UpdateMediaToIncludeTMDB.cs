using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yugen.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMediaToIncludeTMDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_media_media_SuccessorId",
                table: "media");

            migrationBuilder.DropIndex(
                name: "IX_media_SuccessorId",
                table: "media");

            migrationBuilder.RenameColumn(
                name: "SuccessorId",
                table: "media",
                newName: "TMDBSeasonId");

            migrationBuilder.AddColumn<int>(
                name: "TMDBID",
                table: "media",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TMDBType",
                table: "media",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TMDBID",
                table: "media");

            migrationBuilder.DropColumn(
                name: "TMDBType",
                table: "media");

            migrationBuilder.RenameColumn(
                name: "TMDBSeasonId",
                table: "media",
                newName: "SuccessorId");

            migrationBuilder.CreateIndex(
                name: "IX_media_SuccessorId",
                table: "media",
                column: "SuccessorId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_media_media_SuccessorId",
                table: "media",
                column: "SuccessorId",
                principalTable: "media",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
