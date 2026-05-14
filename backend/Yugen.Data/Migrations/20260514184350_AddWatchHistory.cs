using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Yugen.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddWatchHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "watchHistory",
                columns: table => new
                {
                    MediaId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WatchedEpisode = table.Column<int>(type: "integer", nullable: false),
                    UpdatedTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_watchHistory", x => x.MediaId);
                });

            migrationBuilder.CreateTable(
                name: "watchedEpisodes",
                columns: table => new
                {
                    MediaId = table.Column<int>(type: "integer", nullable: false),
                    EpisodeNumber = table.Column<int>(type: "integer", nullable: false),
                    WatchPercentage = table.Column<float>(type: "real", nullable: true),
                    LastWatched = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_watchedEpisodes", x => new { x.MediaId, x.EpisodeNumber });
                    table.ForeignKey(
                        name: "FK_watchedEpisodes_watchHistory_MediaId",
                        column: x => x.MediaId,
                        principalTable: "watchHistory",
                        principalColumn: "MediaId",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "watchedEpisodes");

            migrationBuilder.DropTable(
                name: "watchHistory");
        }
    }
}
