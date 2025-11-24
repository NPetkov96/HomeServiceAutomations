using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataLayer.Migrations
{
    /// <inheritdoc />
    public partial class added_construction_imotbg : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Construction",
                table: "ImotBgApartments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "ImotBgSettings",
                keyColumn: "Id",
                keyValue: 3,
                column: "Value",
                value: "https://www.imot.bg/obiavi/prodazhbi/grad-#CITY#/#CURRENTNEIGHBOURHOOD#/#CONSTRUCTION#");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Construction",
                table: "ImotBgApartments");

            migrationBuilder.UpdateData(
                table: "ImotBgSettings",
                keyColumn: "Id",
                keyValue: 3,
                column: "Value",
                value: "https://www.imot.bg/obiavi/prodazhbi/grad-#CITY#/#CURRENTNEIGHBOURHOOD#");
        }
    }
}
