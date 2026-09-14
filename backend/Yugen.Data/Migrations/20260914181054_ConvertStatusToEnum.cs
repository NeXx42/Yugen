using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yugen.Data.Migrations
{
    /// <inheritdoc />
    public partial class ConvertStatusToEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE media
                ALTER COLUMN "Status" TYPE integer
                USING CASE "Status"
                    WHEN 'FINISHED' THEN 0
                    WHEN 'RELEASING' THEN 1
                    WHEN 'NOT_YET_RELEASED' THEN 2
                    WHEN 'CANCELLED' THEN 3
                    WHEN 'HIATUS' THEN 4

                    WHEN 'Finished Airing' THEN 0
                    WHEN 'Currently Airing' THEN 1
                    WHEN 'Not yet aired' THEN 2

                    ELSE NULL
                END;
            """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "media",
                type: "text",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }
    }
}
