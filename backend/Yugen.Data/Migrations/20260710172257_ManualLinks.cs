using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Yugen.Data.Migrations
{
    /// <inheritdoc />
    public partial class ManualLinks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "manualLinks",
                columns: table => new
                {
                    anilist_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    type = table.Column<string>(type: "text", nullable: true),
                    anidb_id = table.Column<int>(type: "integer", nullable: true),
                    animecountdown_id = table.Column<int>(type: "integer", nullable: true),
                    animenewsnetwork_id = table.Column<int>(type: "integer", nullable: true),
                    anime_planet_id = table.Column<string>(type: "text", nullable: true),
                    anisearch_id = table.Column<int>(type: "integer", nullable: true),
                    imdb_id = table.Column<string>(type: "text", nullable: true),
                    kitsu_id = table.Column<int>(type: "integer", nullable: true),
                    livechart_id = table.Column<int>(type: "integer", nullable: true),
                    mal_id = table.Column<int>(type: "integer", nullable: true),
                    simkl_id = table.Column<int>(type: "integer", nullable: true),
                    themoviedb_id = table.Column<int>(type: "integer", nullable: true),
                    tvdb_id = table.Column<int>(type: "integer", nullable: true),
                    tvdb_season = table.Column<int>(type: "integer", nullable: true),
                    tmdb_season = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_manualLinks", x => x.anilist_id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "manualLinks");
        }
    }
}
