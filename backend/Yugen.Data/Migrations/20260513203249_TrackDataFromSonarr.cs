using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yugen.Data.Migrations
{
    /// <inheritdoc />
    public partial class TrackDataFromSonarr : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Id",
                table: "downloadedMedia",
                newName: "MediaId");

            migrationBuilder.AddColumn<int>(
                name: "downloadedEpisodeEpisodeNumber",
                table: "mediaEpisodes",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "downloadedEpisodeMediaId",
                table: "mediaEpisodes",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AniDbId",
                table: "downloadedMedia",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsMonitored",
                table: "downloadedMedia",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastChecked",
                table: "downloadedMedia",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "sonarrEpisodes",
                columns: table => new
                {
                    MediaId = table.Column<int>(type: "integer", nullable: false),
                    EpisodeNumber = table.Column<int>(type: "integer", nullable: false),
                    sonarrEpisodeId = table.Column<int>(type: "integer", nullable: true),
                    filePath = table.Column<string>(type: "text", nullable: true),
                    JellyfinId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sonarrEpisodes", x => new { x.MediaId, x.EpisodeNumber });
                    table.ForeignKey(
                        name: "FK_sonarrEpisodes_downloadedMedia_MediaId",
                        column: x => x.MediaId,
                        principalTable: "downloadedMedia",
                        principalColumn: "MediaId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_mediaEpisodes_downloadedEpisodeMediaId_downloadedEpisodeEpi~",
                table: "mediaEpisodes",
                columns: new[] { "downloadedEpisodeMediaId", "downloadedEpisodeEpisodeNumber" });

            migrationBuilder.AddForeignKey(
                name: "FK_mediaEpisodes_sonarrEpisodes_downloadedEpisodeMediaId_downl~",
                table: "mediaEpisodes",
                columns: new[] { "downloadedEpisodeMediaId", "downloadedEpisodeEpisodeNumber" },
                principalTable: "sonarrEpisodes",
                principalColumns: new[] { "MediaId", "EpisodeNumber" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_mediaEpisodes_sonarrEpisodes_downloadedEpisodeMediaId_downl~",
                table: "mediaEpisodes");

            migrationBuilder.DropTable(
                name: "sonarrEpisodes");

            migrationBuilder.DropIndex(
                name: "IX_mediaEpisodes_downloadedEpisodeMediaId_downloadedEpisodeEpi~",
                table: "mediaEpisodes");

            migrationBuilder.DropColumn(
                name: "downloadedEpisodeEpisodeNumber",
                table: "mediaEpisodes");

            migrationBuilder.DropColumn(
                name: "downloadedEpisodeMediaId",
                table: "mediaEpisodes");

            migrationBuilder.DropColumn(
                name: "AniDbId",
                table: "downloadedMedia");

            migrationBuilder.DropColumn(
                name: "IsMonitored",
                table: "downloadedMedia");

            migrationBuilder.DropColumn(
                name: "LastChecked",
                table: "downloadedMedia");

            migrationBuilder.RenameColumn(
                name: "MediaId",
                table: "downloadedMedia",
                newName: "Id");
        }
    }
}
