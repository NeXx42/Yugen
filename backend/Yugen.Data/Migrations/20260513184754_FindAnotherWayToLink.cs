using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yugen.Data.Migrations
{
    /// <inheritdoc />
    public partial class FindAnotherWayToLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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
                newName: "AniDBId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AniDBId",
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
    }
}
