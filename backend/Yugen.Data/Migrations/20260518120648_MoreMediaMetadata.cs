using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yugen.Data.Migrations
{
    /// <inheritdoc />
    public partial class MoreMediaMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EpisodeIcon",
                table: "mediaEpisodes",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AverageScore",
                table: "media",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "media",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MeanScore",
                table: "media",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MediaFormat",
                table: "media",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SiteUrl",
                table: "media",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "media",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "thumbnailIcon",
                table: "media",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EpisodeIcon",
                table: "mediaEpisodes");

            migrationBuilder.DropColumn(
                name: "AverageScore",
                table: "media");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "media");

            migrationBuilder.DropColumn(
                name: "MeanScore",
                table: "media");

            migrationBuilder.DropColumn(
                name: "MediaFormat",
                table: "media");

            migrationBuilder.DropColumn(
                name: "SiteUrl",
                table: "media");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "media");

            migrationBuilder.DropColumn(
                name: "thumbnailIcon",
                table: "media");
        }
    }
}
