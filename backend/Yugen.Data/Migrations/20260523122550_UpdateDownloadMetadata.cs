using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yugen.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDownloadMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "sonarrEpisodeId",
                table: "sonarrEpisodes",
                newName: "fileId");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "sonarrEpisodes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "monitored",
                table: "sonarrEpisodes",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Id",
                table: "sonarrEpisodes");

            migrationBuilder.DropColumn(
                name: "monitored",
                table: "sonarrEpisodes");

            migrationBuilder.RenameColumn(
                name: "fileId",
                table: "sonarrEpisodes",
                newName: "sonarrEpisodeId");
        }
    }
}
