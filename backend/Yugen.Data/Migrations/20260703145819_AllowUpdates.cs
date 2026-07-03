using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yugen.Data.Migrations
{
    /// <inheritdoc />
    public partial class AllowUpdates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Hydrated",
                table: "media");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "media",
                newName: "TitleNative");

            migrationBuilder.AddColumn<long>(
                name: "LastUpdated",
                table: "media",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TitleEnglish",
                table: "media",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastUpdated",
                table: "media");

            migrationBuilder.DropColumn(
                name: "TitleEnglish",
                table: "media");

            migrationBuilder.RenameColumn(
                name: "TitleNative",
                table: "media",
                newName: "Title");

            migrationBuilder.AddColumn<bool>(
                name: "Hydrated",
                table: "media",
                type: "boolean",
                nullable: true);
        }
    }
}
