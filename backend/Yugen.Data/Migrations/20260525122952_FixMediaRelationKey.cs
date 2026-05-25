using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yugen.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixMediaRelationKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_mediaRelations",
                table: "mediaRelations");

            migrationBuilder.AddPrimaryKey(
                name: "PK_mediaRelations",
                table: "mediaRelations",
                columns: new[] { "MediaId", "ConnectedMediaId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_mediaRelations",
                table: "mediaRelations");

            migrationBuilder.AddPrimaryKey(
                name: "PK_mediaRelations",
                table: "mediaRelations",
                column: "MediaId");
        }
    }
}
