using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yugen.Data.Migrations
{
    /// <inheritdoc />
    public partial class StoreMediaTags2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Model_MediaTag_media_MediaId",
                table: "Model_MediaTag");

            migrationBuilder.DropForeignKey(
                name: "FK_Model_MediaTag_tags_TagId",
                table: "Model_MediaTag");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Model_MediaTag",
                table: "Model_MediaTag");

            migrationBuilder.RenameTable(
                name: "Model_MediaTag",
                newName: "mediaTags");

            migrationBuilder.RenameIndex(
                name: "IX_Model_MediaTag_TagId",
                table: "mediaTags",
                newName: "IX_mediaTags_TagId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_mediaTags",
                table: "mediaTags",
                columns: new[] { "MediaId", "TagId" });

            migrationBuilder.AddForeignKey(
                name: "FK_mediaTags_media_MediaId",
                table: "mediaTags",
                column: "MediaId",
                principalTable: "media",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_mediaTags_tags_TagId",
                table: "mediaTags",
                column: "TagId",
                principalTable: "tags",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_mediaTags_media_MediaId",
                table: "mediaTags");

            migrationBuilder.DropForeignKey(
                name: "FK_mediaTags_tags_TagId",
                table: "mediaTags");

            migrationBuilder.DropPrimaryKey(
                name: "PK_mediaTags",
                table: "mediaTags");

            migrationBuilder.RenameTable(
                name: "mediaTags",
                newName: "Model_MediaTag");

            migrationBuilder.RenameIndex(
                name: "IX_mediaTags_TagId",
                table: "Model_MediaTag",
                newName: "IX_Model_MediaTag_TagId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Model_MediaTag",
                table: "Model_MediaTag",
                columns: new[] { "MediaId", "TagId" });

            migrationBuilder.AddForeignKey(
                name: "FK_Model_MediaTag_media_MediaId",
                table: "Model_MediaTag",
                column: "MediaId",
                principalTable: "media",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Model_MediaTag_tags_TagId",
                table: "Model_MediaTag",
                column: "TagId",
                principalTable: "tags",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
