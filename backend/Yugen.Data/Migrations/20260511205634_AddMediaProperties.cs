using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yugen.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMediaProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BannerImage",
                table: "media",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CardImageLarge",
                table: "media",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CardImageSmall",
                table: "media",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Colour",
                table: "media",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EpisodeCount",
                table: "media",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MalId",
                table: "media",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BannerImage",
                table: "media");

            migrationBuilder.DropColumn(
                name: "CardImageLarge",
                table: "media");

            migrationBuilder.DropColumn(
                name: "CardImageSmall",
                table: "media");

            migrationBuilder.DropColumn(
                name: "Colour",
                table: "media");

            migrationBuilder.DropColumn(
                name: "EpisodeCount",
                table: "media");

            migrationBuilder.DropColumn(
                name: "MalId",
                table: "media");
        }
    }
}
