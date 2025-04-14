using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FnbReservationSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddIcNoToStaff2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IcNo",
                table: "Staffs",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IcNo",
                table: "Staffs");
        }
    }
}
