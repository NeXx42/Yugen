using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yugen.Data.Migrations
{
    /// <inheritdoc />
    public partial class StoreNextEpisodeTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "NextEpisodeReleaseDate",
                table: "media",
                type: "bigint",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NextEpisodeReleaseDate",
                table: "media");
        }
    }
}
