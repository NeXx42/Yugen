using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yugen.Data.Migrations
{
    /// <inheritdoc />
    public partial class MediaRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "mediaRelations",
                columns: table => new
                {
                    MediaId = table.Column<int>(type: "integer", nullable: false),
                    ConnectedMediaId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mediaRelations", x => x.MediaId);
                    table.ForeignKey(
                        name: "FK_mediaRelations_media_MediaId",
                        column: x => x.MediaId,
                        principalTable: "media",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "mediaRelations");
        }
    }
}
