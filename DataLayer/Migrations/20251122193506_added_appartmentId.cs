using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataLayer.Migrations
{
    /// <inheritdoc />
    public partial class added_appartmentId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApartmentId",
                table: "ImotBgApartments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "ImotBgSettings",
                keyColumn: "Id",
                keyValue: 1,
                column: "Value",
                value: "mladost-1,mladost-2");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApartmentId",
                table: "ImotBgApartments");

            migrationBuilder.UpdateData(
                table: "ImotBgSettings",
                keyColumn: "Id",
                keyValue: 1,
                column: "Value",
                value: "mladost");
        }
    }
}
