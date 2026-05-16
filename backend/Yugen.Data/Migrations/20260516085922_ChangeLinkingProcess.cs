using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yugen.Data.Migrations
{
    /// <inheritdoc />
    public partial class ChangeLinkingProcess : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AniDBId",
                table: "media");

            migrationBuilder.DropColumn(
                name: "MalId",
                table: "media");

            migrationBuilder.DropColumn(
                name: "AniDbId",
                table: "downloadedMedia");

            migrationBuilder.RenameColumn(
                name: "tvdbid",
                table: "links",
                newName: "tvdb_season");

            migrationBuilder.RenameColumn(
                name: "tmdbtv",
                table: "links",
                newName: "tvdb_id");

            migrationBuilder.RenameColumn(
                name: "tmdbseason",
                table: "links",
                newName: "tmdb_season");

            migrationBuilder.RenameColumn(
                name: "defaulttvdbseason",
                table: "links",
                newName: "themoviedb_id");

            migrationBuilder.RenameColumn(
                name: "anidbid",
                table: "links",
                newName: "anilist_id");

            migrationBuilder.AddColumn<int>(
                name: "anidb_id",
                table: "links",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "anime_planet_id",
                table: "links",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "animecountdown_id",
                table: "links",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "animenewsnetwork_id",
                table: "links",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "anisearch_id",
                table: "links",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "imdb_id",
                table: "links",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "kitsu_id",
                table: "links",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "livechart_id",
                table: "links",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "mal_id",
                table: "links",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "simkl_id",
                table: "links",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "type",
                table: "links",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "anidb_id",
                table: "links");

            migrationBuilder.DropColumn(
                name: "anime_planet_id",
                table: "links");

            migrationBuilder.DropColumn(
                name: "animecountdown_id",
                table: "links");

            migrationBuilder.DropColumn(
                name: "animenewsnetwork_id",
                table: "links");

            migrationBuilder.DropColumn(
                name: "anisearch_id",
                table: "links");

            migrationBuilder.DropColumn(
                name: "imdb_id",
                table: "links");

            migrationBuilder.DropColumn(
                name: "kitsu_id",
                table: "links");

            migrationBuilder.DropColumn(
                name: "livechart_id",
                table: "links");

            migrationBuilder.DropColumn(
                name: "mal_id",
                table: "links");

            migrationBuilder.DropColumn(
                name: "simkl_id",
                table: "links");

            migrationBuilder.DropColumn(
                name: "type",
                table: "links");

            migrationBuilder.RenameColumn(
                name: "tvdb_season",
                table: "links",
                newName: "tvdbid");

            migrationBuilder.RenameColumn(
                name: "tvdb_id",
                table: "links",
                newName: "tmdbtv");

            migrationBuilder.RenameColumn(
                name: "tmdb_season",
                table: "links",
                newName: "tmdbseason");

            migrationBuilder.RenameColumn(
                name: "themoviedb_id",
                table: "links",
                newName: "defaulttvdbseason");

            migrationBuilder.RenameColumn(
                name: "anilist_id",
                table: "links",
                newName: "anidbid");

            migrationBuilder.AddColumn<int>(
                name: "AniDBId",
                table: "media",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MalId",
                table: "media",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AniDbId",
                table: "downloadedMedia",
                type: "integer",
                nullable: true);
        }
    }
}
