using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Manga.Repository.Migrations
{
    /// <inheritdoc />
    public partial class Fix_mangakataskStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "Readers");

            migrationBuilder.RenameColumn(
                name: "LastName",
                table: "Readers",
                newName: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Readers",
                newName: "LastName");

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "Readers",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);
        }
    }
}
