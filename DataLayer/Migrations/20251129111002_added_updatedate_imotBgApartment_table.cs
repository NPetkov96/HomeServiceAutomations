using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataLayer.Migrations
{
    /// <inheritdoc />
    public partial class added_updatedate_imotBgApartment_table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "ImotBgApartments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "ImotBgSettings",
                keyColumn: "Id",
                keyValue: 1,
                column: "Value",
                value: "mladost-1,mladost-1a,mladost-2,mladost-3,mladost-4,malinova-dolina,musagenitsa,darvenitsa,eksperimentalен");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "ImotBgApartments");

            migrationBuilder.UpdateData(
                table: "ImotBgSettings",
                keyColumn: "Id",
                keyValue: 1,
                column: "Value",
                value: "mladost-1,mladost-2");
        }
    }
}
