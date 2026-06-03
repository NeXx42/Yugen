using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yugen.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EventType",
                table: "notifications");

            migrationBuilder.AlterColumn<int>(
                name: "MediaId",
                table: "notifications",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<string>(
                name: "EventName",
                table: "notifications",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "MediaEpisode",
                table: "notifications",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Source",
                table: "notifications",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EventName",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "MediaEpisode",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "Source",
                table: "notifications");

            migrationBuilder.AlterColumn<int>(
                name: "MediaId",
                table: "notifications",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EventType",
                table: "notifications",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
