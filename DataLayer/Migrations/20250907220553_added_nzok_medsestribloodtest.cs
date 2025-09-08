using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataLayer.Migrations
{
    /// <inheritdoc />
    public partial class added_nzok_medsestribloodtest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "MedSestriBloodTests",
                columns: new[] { "Id", "BngPrice", "EuroPrice", "HasPriority", "Name" },
                values: new object[] { 999, 0.0, 0.0, true, "НЗОК" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "MedSestriBloodTests",
                keyColumn: "Id",
                keyValue: 999);
        }
    }
}
