using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Yugen.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBookmarks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "bookmarkTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bookmarkTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "userBookmarks",
                columns: table => new
                {
                    MediaId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    BookmarkId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_userBookmarks", x => new { x.MediaId, x.UserId });
                    table.ForeignKey(
                        name: "FK_userBookmarks_bookmarkTypes_BookmarkId",
                        column: x => x.BookmarkId,
                        principalTable: "bookmarkTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "bookmarkTypes",
                columns: new[] { "Id", "Title" },
                values: new object[,]
                {
                    { 1, "Watching" },
                    { 2, "OnHold" },
                    { 3, "Planning" },
                    { 4, "Completed" },
                    { 5, "Dropped" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_userBookmarks_BookmarkId",
                table: "userBookmarks",
                column: "BookmarkId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "userBookmarks");

            migrationBuilder.DropTable(
                name: "bookmarkTypes");
        }
    }
}
