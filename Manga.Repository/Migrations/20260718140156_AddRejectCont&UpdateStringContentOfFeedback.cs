using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Manga.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddRejectContUpdateStringContentOfFeedback : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RejectCount",
                table: "MangaTasks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "Feedbacks",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(3000)",
                oldMaxLength: 3000);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RejectCount",
                table: "MangaTasks");

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "Feedbacks",
                type: "character varying(3000)",
                maxLength: 3000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");
        }
    }
}
