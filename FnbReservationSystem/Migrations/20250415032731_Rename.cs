using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FnbReservationSystem.Migrations
{
    /// <inheritdoc />
    public partial class Rename : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_NoShow",
                table: "NoShow");

            migrationBuilder.RenameTable(
                name: "NoShow",
                newName: "NoShows");

            migrationBuilder.AddPrimaryKey(
                name: "PK_NoShows",
                table: "NoShows",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_NoShows",
                table: "NoShows");

            migrationBuilder.RenameTable(
                name: "NoShows",
                newName: "NoShow");

            migrationBuilder.AddPrimaryKey(
                name: "PK_NoShow",
                table: "NoShow",
                column: "Id");
        }
    }
}
