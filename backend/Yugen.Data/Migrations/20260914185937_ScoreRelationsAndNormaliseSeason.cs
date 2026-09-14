using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yugen.Data.Migrations
{
    /// <inheritdoc />
    public partial class ScoreRelationsAndNormaliseSeason : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RelationScore",
                table: "mediaRelations",
                type: "integer",
                nullable: true);

            migrationBuilder.Sql("""
                ALTER TABLE media
                ALTER COLUMN "Season" TYPE integer
                USING CASE "Season"
                    WHEN 'winter' THEN 0
                    WHEN 'spring' THEN 1
                    WHEN 'summer' THEN 2
                    WHEN 'fall' THEN 3
                    ELSE NULL
                END;
            """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RelationScore",
                table: "mediaRelations");

            migrationBuilder.AlterColumn<string>(
                name: "Season",
                table: "media",
                type: "text",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }
    }
}
