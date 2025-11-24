using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataLayer.Migrations
{
    /// <inheritdoc />
    public partial class addedsettingsimotbg : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "ImotBgSettings",
                columns: new[] { "Id", "Name", "Value" },
                values: new object[] { 4, "ConstructionType", "PANEL,TUHLA,EPK" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ImotBgSettings",
                keyColumn: "Id",
                keyValue: 4);
        }
    }
}
