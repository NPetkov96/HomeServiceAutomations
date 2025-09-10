using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataLayer.Migrations
{
    /// <inheritdoc />
    public partial class added_isOverdue_property : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsOverdue",
                table: "MedSestriCatheters",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsOverdue",
                table: "MedSestriCatheters");
        }
    }
}
