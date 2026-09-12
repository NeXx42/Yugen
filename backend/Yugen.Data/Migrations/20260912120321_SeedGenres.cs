using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yugen.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedGenres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "genres",
                column: "Genre",
                values: new object[]
                {
                    "Action",
                    "Adventure",
                    "Comedy",
                    "Drama",
                    "Ecchi",
                    "Fantasy",
                    "Hentai",
                    "Horror",
                    "Mahou Shoujo",
                    "Mecha",
                    "Music",
                    "Mystery",
                    "Psychological",
                    "Romance",
                    "Sci-Fi",
                    "Slice of Life",
                    "Sports",
                    "Supernatural",
                    "Thriller"
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "genres",
                keyColumn: "Genre",
                keyValue: "Action");

            migrationBuilder.DeleteData(
                table: "genres",
                keyColumn: "Genre",
                keyValue: "Adventure");

            migrationBuilder.DeleteData(
                table: "genres",
                keyColumn: "Genre",
                keyValue: "Comedy");

            migrationBuilder.DeleteData(
                table: "genres",
                keyColumn: "Genre",
                keyValue: "Drama");

            migrationBuilder.DeleteData(
                table: "genres",
                keyColumn: "Genre",
                keyValue: "Ecchi");

            migrationBuilder.DeleteData(
                table: "genres",
                keyColumn: "Genre",
                keyValue: "Fantasy");

            migrationBuilder.DeleteData(
                table: "genres",
                keyColumn: "Genre",
                keyValue: "Hentai");

            migrationBuilder.DeleteData(
                table: "genres",
                keyColumn: "Genre",
                keyValue: "Horror");

            migrationBuilder.DeleteData(
                table: "genres",
                keyColumn: "Genre",
                keyValue: "Mahou Shoujo");

            migrationBuilder.DeleteData(
                table: "genres",
                keyColumn: "Genre",
                keyValue: "Mecha");

            migrationBuilder.DeleteData(
                table: "genres",
                keyColumn: "Genre",
                keyValue: "Music");

            migrationBuilder.DeleteData(
                table: "genres",
                keyColumn: "Genre",
                keyValue: "Mystery");

            migrationBuilder.DeleteData(
                table: "genres",
                keyColumn: "Genre",
                keyValue: "Psychological");

            migrationBuilder.DeleteData(
                table: "genres",
                keyColumn: "Genre",
                keyValue: "Romance");

            migrationBuilder.DeleteData(
                table: "genres",
                keyColumn: "Genre",
                keyValue: "Sci-Fi");

            migrationBuilder.DeleteData(
                table: "genres",
                keyColumn: "Genre",
                keyValue: "Slice of Life");

            migrationBuilder.DeleteData(
                table: "genres",
                keyColumn: "Genre",
                keyValue: "Sports");

            migrationBuilder.DeleteData(
                table: "genres",
                keyColumn: "Genre",
                keyValue: "Supernatural");

            migrationBuilder.DeleteData(
                table: "genres",
                keyColumn: "Genre",
                keyValue: "Thriller");
        }
    }
}
