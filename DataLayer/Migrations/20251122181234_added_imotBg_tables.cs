using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DataLayer.Migrations
{
    /// <inheritdoc />
    public partial class added_imotBg_tables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ImotBgApartments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Neighbour = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SquareMetres = table.Column<double>(type: "float", nullable: false),
                    Price = table.Column<double>(type: "float", nullable: false),
                    PricePerSqMetre = table.Column<double>(type: "float", nullable: false),
                    Floor = table.Column<int>(type: "int", nullable: false),
                    MoreInformation = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    URl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsNew = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImotBgApartments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ImotBgSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImotBgSettings", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "ImotBgSettings",
                columns: new[] { "Id", "Name", "Value" },
                values: new object[,]
                {
                    { 1, "SerachNeighbour", "mladost" },
                    { 2, "SerachCity", "sofiya" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ImotBgApartments");

            migrationBuilder.DropTable(
                name: "ImotBgSettings");
        }
    }
}
