using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Manga.Repository.Migrations
{
    /// <inheritdoc />
    public partial class addEntityReaderandMangaTaskStatusassistantcanaccepttaskupdatechaptervotingandusersession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChapterVotings_Users_ReaderId",
                table: "ChapterVotings");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "UserSessions",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "ReaderId",
                table: "UserSessions",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Readers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    FirstName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    LastName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    AvatarUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    GoogleAccountId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Readers", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserSessions_ReaderId",
                table: "UserSessions",
                column: "ReaderId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_UserSession_Exclusive_User_Or_Reader",
                table: "UserSessions",
                sql: "(\"UserId\" IS NULL AND \"ReaderId\" IS NOT NULL) OR (\"UserId\" IS NOT NULL AND \"ReaderId\" IS NULL)");

            migrationBuilder.CreateIndex(
                name: "IX_Readers_Email",
                table: "Readers",
                column: "Email",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ChapterVotings_Readers_ReaderId",
                table: "ChapterVotings",
                column: "ReaderId",
                principalTable: "Readers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserSessions_Readers_ReaderId",
                table: "UserSessions",
                column: "ReaderId",
                principalTable: "Readers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChapterVotings_Readers_ReaderId",
                table: "ChapterVotings");

            migrationBuilder.DropForeignKey(
                name: "FK_UserSessions_Readers_ReaderId",
                table: "UserSessions");

            migrationBuilder.DropTable(
                name: "Readers");

            migrationBuilder.DropIndex(
                name: "IX_UserSessions_ReaderId",
                table: "UserSessions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_UserSession_Exclusive_User_Or_Reader",
                table: "UserSessions");

            migrationBuilder.DropColumn(
                name: "ReaderId",
                table: "UserSessions");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "UserSessions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ChapterVotings_Users_ReaderId",
                table: "ChapterVotings",
                column: "ReaderId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
