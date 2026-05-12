using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yugen.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEpisodeMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EpisodeTitle",
                table: "mediaEpisodes",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFiller",
                table: "mediaEpisodes",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsRecap",
                table: "mediaEpisodes",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<float>(
                name: "Score",
                table: "mediaEpisodes",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Hydrated",
                table: "media",
                type: "boolean",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EpisodeTitle",
                table: "mediaEpisodes");

            migrationBuilder.DropColumn(
                name: "IsFiller",
                table: "mediaEpisodes");

            migrationBuilder.DropColumn(
                name: "IsRecap",
                table: "mediaEpisodes");

            migrationBuilder.DropColumn(
                name: "Score",
                table: "mediaEpisodes");

            migrationBuilder.DropColumn(
                name: "Hydrated",
                table: "media");
        }
    }
}
