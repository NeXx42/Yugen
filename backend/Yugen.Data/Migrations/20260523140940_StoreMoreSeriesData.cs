using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yugen.Data.Migrations
{
    /// <inheritdoc />
    public partial class StoreMoreSeriesData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ExternalQuality",
                table: "downloadedMedia",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalRoot",
                table: "downloadedMedia",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProviderId",
                table: "downloadedMedia",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExternalQuality",
                table: "downloadedMedia");

            migrationBuilder.DropColumn(
                name: "ExternalRoot",
                table: "downloadedMedia");

            migrationBuilder.DropColumn(
                name: "ProviderId",
                table: "downloadedMedia");
        }
    }
}
