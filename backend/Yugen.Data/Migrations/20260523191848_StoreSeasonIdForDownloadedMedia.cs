using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yugen.Data.Migrations
{
    /// <inheritdoc />
    public partial class StoreSeasonIdForDownloadedMedia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SeasonId",
                table: "downloadedMedia",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SeasonId",
                table: "downloadedMedia");
        }
    }
}
