using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Manga.Repository.Migrations
{
    /// <inheritdoc />
    public partial class UpdateLeaderBoard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Leaderboards_SeriesId_RankingPeriod",
                table: "Leaderboards");

            migrationBuilder.AlterColumn<int>(
                name: "TotalVotes",
                table: "Leaderboards",
                type: "integer",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(5,2)");

            migrationBuilder.AlterColumn<string>(
                name: "RankingPeriod",
                table: "Leaderboards",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<double>(
                name: "AverageRate",
                table: "Leaderboards",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<DateTime>(
                name: "PeriodEnd",
                table: "Leaderboards",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "PeriodStart",
                table: "Leaderboards",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Leaderboards",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Leaderboards_SeriesId_Type_PeriodStart_PeriodEnd",
                table: "Leaderboards",
                columns: new[] { "SeriesId", "Type", "PeriodStart", "PeriodEnd" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Leaderboards_SeriesId_Type_PeriodStart_PeriodEnd",
                table: "Leaderboards");

            migrationBuilder.DropColumn(
                name: "AverageRate",
                table: "Leaderboards");

            migrationBuilder.DropColumn(
                name: "PeriodEnd",
                table: "Leaderboards");

            migrationBuilder.DropColumn(
                name: "PeriodStart",
                table: "Leaderboards");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Leaderboards");

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalVotes",
                table: "Leaderboards",
                type: "numeric(5,2)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "RankingPeriod",
                table: "Leaderboards",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Leaderboards_SeriesId_RankingPeriod",
                table: "Leaderboards",
                columns: new[] { "SeriesId", "RankingPeriod" },
                unique: true);
        }
    }
}
