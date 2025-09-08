using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataLayer.Migrations
{
    /// <inheritdoc />
    public partial class added_BloodTestList_to_Patients : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MedSestriPatientsBloodTests",
                columns: table => new
                {
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    BloodTestId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedSestriPatientsBloodTests", x => new { x.PatientId, x.BloodTestId });
                    table.ForeignKey(
                        name: "FK_MedSestriPatientsBloodTests_MedSestriBloodTests_BloodTestId",
                        column: x => x.BloodTestId,
                        principalTable: "MedSestriBloodTests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MedSestriPatientsBloodTests_MedSestriPatients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "MedSestriPatients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MedSestriPatientsBloodTests_BloodTestId",
                table: "MedSestriPatientsBloodTests",
                column: "BloodTestId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MedSestriPatientsBloodTests");
        }
    }
}
