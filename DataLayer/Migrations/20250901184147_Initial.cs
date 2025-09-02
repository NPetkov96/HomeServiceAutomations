using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DataLayer.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CampaignClients",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignClients", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CampaignEmails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmailId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Client = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CampaignId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Body = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    ReceivedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    EmailStatus = table.Column<int>(type: "int", nullable: false),
                    AttachmentPath = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignEmails", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CampaignPlatforms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CPC = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CostPerPostEngagment = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignPlatforms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CampaignSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CampaignAssets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Client = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Product = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CampaignId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Platform = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Target = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Optimization = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Formats = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Period = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Budget = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CampaignEmailId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignAssets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CampaignAssets_CampaignEmails_CampaignEmailId",
                        column: x => x.CampaignEmailId,
                        principalTable: "CampaignEmails",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CampaignClientPlatforms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClientId = table.Column<int>(type: "int", nullable: false),
                    PlatformId = table.Column<int>(type: "int", nullable: false),
                    PercentFee = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignClientPlatforms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CampaignClientPlatforms_CampaignClients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "CampaignClients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CampaignClientPlatforms_CampaignPlatforms_PlatformId",
                        column: x => x.PlatformId,
                        principalTable: "CampaignPlatforms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "CampaignClients",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Menta Peshtera" },
                    { 2, "Peshterska rakia" },
                    { 3, "Alaska" },
                    { 4, "Flirt" },
                    { 5, "Kailushka" },
                    { 6, "Mastika Peshtera" },
                    { 7, "Slivenska perla" },
                    { 8, "Black Ram" },
                    { 9, "Sixth Sense Gin" },
                    { 10, "Straldjanska" },
                    { 11, "Abopharma" },
                    { 12, "Citroen" },
                    { 13, "Peugeot" },
                    { 14, "KiaMotors" },
                    { 15, "Italia Motors" },
                    { 16, "SFA Sofia 1" },
                    { 17, "SFA Sofia 2" },
                    { 18, "SFA Sofia 3" },
                    { 19, "SFA Sofia 4" },
                    { 20, "SFA Ruse" },
                    { 21, "SFA VTurnovo" },
                    { 22, "SFA Okazion" },
                    { 23, "SFA" },
                    { 24, "SFA Broker" },
                    { 25, "Subaru" },
                    { 26, "Honda" },
                    { 27, "Smart Electric Tech" },
                    { 28, "HMD" },
                    { 29, "MagicalFlowersBG" },
                    { 30, "eBay" },
                    { 31, "OraSi Bulgaria" },
                    { 32, "FIZI Publishing" },
                    { 33, "EMSE Publishing" },
                    { 34, "Melitta" },
                    { 35, "Pitagor School" },
                    { 36, "AYA Estate" }
                });

            migrationBuilder.InsertData(
                table: "CampaignPlatforms",
                columns: new[] { "Id", "CPC", "CostPerPostEngagment", "Name" },
                values: new object[,]
                {
                    { 1, null, null, "Google" },
                    { 2, null, null, "EasyAds" },
                    { 3, null, null, "YouTube" },
                    { 4, null, null, "Netinfo" },
                    { 5, null, null, "Influencer marketing" },
                    { 6, null, null, "Facebook" },
                    { 7, null, null, "TikTok" },
                    { 8, null, null, "Instagram" },
                    { 9, null, null, "Facebook & Instagram" }
                });

            migrationBuilder.InsertData(
                table: "CampaignSettings",
                columns: new[] { "Id", "Name", "Value" },
                values: new object[,]
                {
                    { 1, "LastDateTimeReadEmails", "2025-08-08-08-00-00-0" },
                    { 2, "AttachmentPath", "C:\\NoniApp\\WorkHardNoni\\Attachments" },
                    { 3, "ErrorFilePth", "C:\\NoniApp\\WorkHardNoni\\logs\\Errors.txt" },
                    { 4, "SavingAttachmentsPath", "C:\\NoniApp\\WorkHardNoni\\Attachments" },
                    { 5, "CredentialsPath", "C:\\Users\\Nikolay Petkov\\source\\repos\\HomeService\\CredentialsToken\\WorkHardNoniEmail\\credentials.json" },
                    { 6, "MetaAccessToken", "" },
                    { 7, "FirstClientsPage", "https://graph.facebook.com/v23.0/2865723520304947/adaccounts?fields=account_id,id,name&access_token=<ACCESS_TOKEN>%0A&limit=100" },
                    { 8, "AccountInfomationCPC", "https://graph.facebook.com/v23.0/<ACCOUNT_ID>/insights?fields=campaign_id,campaign_name,cpc,objective,actions,spend&breakdowns=publisher_platform,platform_position&date_preset=last_90d&level=campaign&access_token=<ACCESS_TOKEN>" },
                    { 9, "LastDateTimeKPIResult", "2025-08-08-08-00-00-0" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_CampaignAssets_CampaignEmailId",
                table: "CampaignAssets",
                column: "CampaignEmailId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignClientPlatforms_ClientId",
                table: "CampaignClientPlatforms",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignClientPlatforms_PlatformId",
                table: "CampaignClientPlatforms",
                column: "PlatformId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CampaignAssets");

            migrationBuilder.DropTable(
                name: "CampaignClientPlatforms");

            migrationBuilder.DropTable(
                name: "CampaignSettings");

            migrationBuilder.DropTable(
                name: "CampaignEmails");

            migrationBuilder.DropTable(
                name: "CampaignClients");

            migrationBuilder.DropTable(
                name: "CampaignPlatforms");
        }
    }
}
