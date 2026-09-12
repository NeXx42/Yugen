using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Yugen.Data.Migrations
{
    /// <inheritdoc />
    public partial class makeProviderIndependant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_links",
                table: "links");

            migrationBuilder.Sql("""
                ALTER TABLE links
                ALTER COLUMN anilist_id DROP IDENTITY;
            """);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "media",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<int>(
                name: "anilist_id",
                table: "links",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "MediaId",
                table: "links",
                type: "integer",
                nullable: false)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_links",
                table: "links",
                column: "MediaId");

            migrationBuilder.AddForeignKey(
                name: "FK_media_links_Id",
                table: "media",
                column: "Id",
                principalTable: "links",
                principalColumn: "MediaId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_media_links_Id",
                table: "media");

            migrationBuilder.DropPrimaryKey(
                name: "PK_links",
                table: "links");

            migrationBuilder.DropColumn(
                name: "MediaId",
                table: "links");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "media",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<int>(
                name: "anilist_id",
                table: "links",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_links",
                table: "links",
                column: "anilist_id");
        }
    }
}
