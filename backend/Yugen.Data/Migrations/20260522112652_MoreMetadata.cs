using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yugen.Data.Migrations
{
    /// <inheritdoc />
    public partial class MoreMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "EpisodeCount",
                table: "media",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "Duration",
                table: "media",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "EndDate",
                table: "media",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Season",
                table: "media",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "StartDate",
                table: "media",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Year",
                table: "media",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Duration",
                table: "media");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "media");

            migrationBuilder.DropColumn(
                name: "Season",
                table: "media");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "media");

            migrationBuilder.DropColumn(
                name: "Year",
                table: "media");

            migrationBuilder.AlterColumn<int>(
                name: "EpisodeCount",
                table: "media",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }
    }
}
