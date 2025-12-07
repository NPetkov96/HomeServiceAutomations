using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataLayer.Migrations
{
    /// <inheritdoc />
    public partial class added_ImotBgStatistics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "ImotBgApartments");

            migrationBuilder.CreateTable(
                name: "ImotBgStatistics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompareAverage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Average = table.Column<double>(type: "float", nullable: false),
                    CompareMladost = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Mladost = table.Column<double>(type: "float", nullable: false),
                    CompareMladostTuhla = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MladostTuhla = table.Column<double>(type: "float", nullable: false),
                    CompareMladostPanel = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MladostPanel = table.Column<double>(type: "float", nullable: false),
                    CompareMalinovaDolina = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MalinovaDolina = table.Column<double>(type: "float", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImotBgStatistics", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "ImotBgSettings",
                keyColumn: "Id",
                keyValue: 1,
                column: "Value",
                value: "mladost-1,mladost-1a,mladost-2,mladost-3,mladost-4,malinova-dolina,musagenitsa,darvenitsa,eksperimentalen");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ImotBgStatistics");

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "ImotBgApartments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "ImotBgSettings",
                keyColumn: "Id",
                keyValue: 1,
                column: "Value",
                value: "mladost-1,mladost-1a,mladost-2,mladost-3,mladost-4,malinova-dolina,musagenitsa,darvenitsa,eksperimentalен");
        }
    }
}
