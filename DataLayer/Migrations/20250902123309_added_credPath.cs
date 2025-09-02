using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataLayer.Migrations
{
    /// <inheritdoc />
    public partial class added_credPath : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "CampaignSettings",
                columns: new[] { "Id", "Name", "Value" },
                values: new object[] { 10, "GoogleAuth2TokenResposnse", "C:\\Users\\Nikolay Petkov\\source\\repos\\HomeService\\HomeService\\bin\\Debug\\net9.0\\token.json\\Google.Apis.Auth.OAuth2.Responses.TokenResponse-user\\Google.Apis.Auth.OAuth2.Responses.TokenResponse-user" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "CampaignSettings",
                keyColumn: "Id",
                keyValue: 10);
        }
    }
}
