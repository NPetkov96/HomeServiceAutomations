using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataLayer.Migrations
{
    /// <inheritdoc />
    public partial class addedsettingsSkippingTitleWordsimotbg : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "ImotBgSettings",
                columns: new[] { "Id", "Name", "Value" },
                values: new object[] { 5, "SkippingTitleWords", "офис,ателие,таван,фабрика,завод,магазин,фитнес,заведение" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ImotBgSettings",
                keyColumn: "Id",
                keyValue: 5);
        }
    }
}
