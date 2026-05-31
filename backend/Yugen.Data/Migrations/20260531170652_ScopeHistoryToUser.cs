using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Yugen.Data.Migrations
{
    /// <inheritdoc />
    public partial class ScopeHistoryToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_watchedEpisodes_watchHistory_MediaId",
                table: "watchedEpisodes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_watchHistory",
                table: "watchHistory");

            migrationBuilder.RenameColumn(
                name: "WatchedEpisode",
                table: "watchHistory",
                newName: "LastWatchedEpisodeNumber");

            migrationBuilder.RenameColumn(
                name: "MediaId",
                table: "watchedEpisodes",
                newName: "HistoryId");

            migrationBuilder.AlterColumn<int>(
                name: "MediaId",
                table: "watchHistory",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "watchHistory",
                type: "integer",
                nullable: false,
                defaultValue: 0)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "watchHistory",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_watchHistory",
                table: "watchHistory",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_watchedEpisodes_watchHistory_HistoryId",
                table: "watchedEpisodes",
                column: "HistoryId",
                principalTable: "watchHistory",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_watchedEpisodes_watchHistory_HistoryId",
                table: "watchedEpisodes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_watchHistory",
                table: "watchHistory");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "watchHistory");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "watchHistory");

            migrationBuilder.RenameColumn(
                name: "LastWatchedEpisodeNumber",
                table: "watchHistory",
                newName: "WatchedEpisode");

            migrationBuilder.RenameColumn(
                name: "HistoryId",
                table: "watchedEpisodes",
                newName: "MediaId");

            migrationBuilder.AlterColumn<int>(
                name: "MediaId",
                table: "watchHistory",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_watchHistory",
                table: "watchHistory",
                column: "MediaId");

            migrationBuilder.AddForeignKey(
                name: "FK_watchedEpisodes_watchHistory_MediaId",
                table: "watchedEpisodes",
                column: "MediaId",
                principalTable: "watchHistory",
                principalColumn: "MediaId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
