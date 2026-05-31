using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yugen.Data.Migrations
{
    /// <inheritdoc />
    public partial class SaveGenres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_mediaEpisodes_sonarrEpisodes_downloadedEpisodeMediaId_downl~",
                table: "mediaEpisodes");

            migrationBuilder.DropForeignKey(
                name: "FK_sonarrEpisodes_downloadedMedia_MediaId",
                table: "sonarrEpisodes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_sonarrEpisodes",
                table: "sonarrEpisodes");

            migrationBuilder.RenameTable(
                name: "sonarrEpisodes",
                newName: "downloadedEpisodes");

            migrationBuilder.AddPrimaryKey(
                name: "PK_downloadedEpisodes",
                table: "downloadedEpisodes",
                columns: new[] { "MediaId", "EpisodeNumber" });

            migrationBuilder.CreateTable(
                name: "Model_MediaGenre",
                columns: table => new
                {
                    MediaId = table.Column<int>(type: "integer", nullable: false),
                    Genre = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Model_MediaGenre", x => new { x.MediaId, x.Genre });
                    table.ForeignKey(
                        name: "FK_Model_MediaGenre_media_MediaId",
                        column: x => x.MediaId,
                        principalTable: "media",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_downloadedEpisodes_downloadedMedia_MediaId",
                table: "downloadedEpisodes",
                column: "MediaId",
                principalTable: "downloadedMedia",
                principalColumn: "MediaId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_mediaEpisodes_downloadedEpisodes_downloadedEpisodeMediaId_d~",
                table: "mediaEpisodes",
                columns: new[] { "downloadedEpisodeMediaId", "downloadedEpisodeEpisodeNumber" },
                principalTable: "downloadedEpisodes",
                principalColumns: new[] { "MediaId", "EpisodeNumber" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_downloadedEpisodes_downloadedMedia_MediaId",
                table: "downloadedEpisodes");

            migrationBuilder.DropForeignKey(
                name: "FK_mediaEpisodes_downloadedEpisodes_downloadedEpisodeMediaId_d~",
                table: "mediaEpisodes");

            migrationBuilder.DropTable(
                name: "Model_MediaGenre");

            migrationBuilder.DropPrimaryKey(
                name: "PK_downloadedEpisodes",
                table: "downloadedEpisodes");

            migrationBuilder.RenameTable(
                name: "downloadedEpisodes",
                newName: "sonarrEpisodes");

            migrationBuilder.AddPrimaryKey(
                name: "PK_sonarrEpisodes",
                table: "sonarrEpisodes",
                columns: new[] { "MediaId", "EpisodeNumber" });

            migrationBuilder.AddForeignKey(
                name: "FK_mediaEpisodes_sonarrEpisodes_downloadedEpisodeMediaId_downl~",
                table: "mediaEpisodes",
                columns: new[] { "downloadedEpisodeMediaId", "downloadedEpisodeEpisodeNumber" },
                principalTable: "sonarrEpisodes",
                principalColumns: new[] { "MediaId", "EpisodeNumber" });

            migrationBuilder.AddForeignKey(
                name: "FK_sonarrEpisodes_downloadedMedia_MediaId",
                table: "sonarrEpisodes",
                column: "MediaId",
                principalTable: "downloadedMedia",
                principalColumn: "MediaId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
