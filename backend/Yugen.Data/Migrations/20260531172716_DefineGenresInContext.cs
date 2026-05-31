using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yugen.Data.Migrations
{
    /// <inheritdoc />
    public partial class DefineGenresInContext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Model_MediaGenre_media_MediaId",
                table: "Model_MediaGenre");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Model_MediaGenre",
                table: "Model_MediaGenre");

            migrationBuilder.RenameTable(
                name: "Model_MediaGenre",
                newName: "mediaGenres");

            migrationBuilder.AddPrimaryKey(
                name: "PK_mediaGenres",
                table: "mediaGenres",
                columns: new[] { "MediaId", "Genre" });

            migrationBuilder.AddForeignKey(
                name: "FK_mediaGenres_media_MediaId",
                table: "mediaGenres",
                column: "MediaId",
                principalTable: "media",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_mediaGenres_media_MediaId",
                table: "mediaGenres");

            migrationBuilder.DropPrimaryKey(
                name: "PK_mediaGenres",
                table: "mediaGenres");

            migrationBuilder.RenameTable(
                name: "mediaGenres",
                newName: "Model_MediaGenre");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Model_MediaGenre",
                table: "Model_MediaGenre",
                columns: new[] { "MediaId", "Genre" });

            migrationBuilder.AddForeignKey(
                name: "FK_Model_MediaGenre_media_MediaId",
                table: "Model_MediaGenre",
                column: "MediaId",
                principalTable: "media",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
