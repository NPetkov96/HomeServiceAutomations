using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataLayer.Migrations
{
    /// <inheritdoc />
    public partial class addedsettingsSkippingTitleWords2imotbg : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "ImotBgSettings",
                keyColumn: "Id",
                keyValue: 5,
                column: "Value",
                value: "офис,ателие,таван,фабрика,завод,магазин,фитнес,заведение,самостоятелна,сграда,многостаен,мезонет,търговски,комплекс,къща,склад");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "ImotBgSettings",
                keyColumn: "Id",
                keyValue: 5,
                column: "Value",
                value: "офис,ателие,таван,фабрика,завод,магазин,фитнес,заведение");
        }
    }
}
