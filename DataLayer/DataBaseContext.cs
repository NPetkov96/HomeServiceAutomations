using DataLayer.Models;
using DataLayer.Models.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DataLayer
{
    public class DataBaseContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=PETKOV;Database=MyDbContext;Trusted_Connection=True;TrustServerCertificate=True");
        }

        public DbSet<Settings> Settings { get; set; }
        public DbSet<CampaignEmail> CampaignEmails { get; set; }
        public DbSet<CampaignSettings> CampaignSettings { get; set; }
        public DbSet<CampaignAsset> CampaignAssets { get; set; }
        public DbSet<CampaignClient> CampaignClients { get; set; }
        public DbSet<CampaignPlatform> CampaignPlatforms { get; set; }
        public DbSet<CampaignClientPlatform> CampaignClientPlatforms { get; set; }
        public DbSet<MedSestriBloodTest> MedSestriBloodTests { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Settings>().HasData(
                new Settings { Id = 1, Name = "LogsPath", Value = "C:\\HomeService\\Logs" }
            );

            modelBuilder.Entity<CampaignSettings>().HasData(
                new CampaignSettings { Id = 1, Name = "LastDateTimeReadEmails", Value = "2025-08-08-08-00-00-0" },
                new CampaignSettings { Id = 2, Name = "AttachmentPath", Value = "C:\\NoniApp\\WorkHardNoni\\Attachments" },
                new CampaignSettings { Id = 3, Name = "ErrorFilePth", Value = "C:\\NoniApp\\WorkHardNoni\\logs\\Errors.txt" },
                new CampaignSettings { Id = 4, Name = "SavingAttachmentsPath", Value = "C:\\NoniApp\\WorkHardNoni\\Attachments" },
                new CampaignSettings { Id = 5, Name = "CredentialsPath", Value = "C:\\Users\\Nikolay Petkov\\source\\repos\\HomeService\\CredentialsToken\\WorkHardNoniEmail\\credentials.json" },
                new CampaignSettings { Id = 6, Name = "MetaAccessToken", Value = "" },
                new CampaignSettings { Id = 7, Name = "FirstClientsPage", Value = "https://graph.facebook.com/v23.0/2865723520304947/adaccounts?fields=account_id,id,name&access_token=<ACCESS_TOKEN>%0A&limit=100" },
                new CampaignSettings { Id = 8, Name = "AccountInfomationCPC", Value = "https://graph.facebook.com/v23.0/<ACCOUNT_ID>/insights?fields=campaign_id,campaign_name,cpc,objective,actions,spend&breakdowns=publisher_platform,platform_position&date_preset=last_90d&level=campaign&access_token=<ACCESS_TOKEN>" },
                new CampaignSettings { Id = 9, Name = "LastDateTimeKPIResult", Value = "2025-08-08-08-00-00-0" },
                new CampaignSettings { Id = 10, Name = "GoogleAuth2TokenResposnse", Value = "C:\\Users\\Nikolay Petkov\\source\\repos\\HomeService\\HomeService\\bin\\Debug\\net9.0\\token.json\\Google.Apis.Auth.OAuth2.Responses.TokenResponse-user\\Google.Apis.Auth.OAuth2.Responses.TokenResponse-user" }

            );

            modelBuilder.Entity<CampaignPlatform>().HasData(
                new CampaignPlatform { Id = 1, Name = "Google" },
                new CampaignPlatform { Id = 2, Name = "EasyAds" },
                new CampaignPlatform { Id = 3, Name = "YouTube" },
                new CampaignPlatform { Id = 4, Name = "Netinfo" },
                new CampaignPlatform { Id = 5, Name = "Influencer marketing" },
                new CampaignPlatform { Id = 6, Name = "Facebook" },
                new CampaignPlatform { Id = 7, Name = "TikTok" },
                new CampaignPlatform { Id = 8, Name = "Instagram" },
                new CampaignPlatform { Id = 9, Name = "Facebook & Instagram" }
            );
            modelBuilder.Entity<CampaignClient>().HasData(
               new CampaignClient { Id = 1, Name = "Menta Peshtera" },
               new CampaignClient { Id = 2, Name = "Peshterska rakia" },
               new CampaignClient { Id = 3, Name = "Alaska" },
               new CampaignClient { Id = 4, Name = "Flirt" },
               new CampaignClient { Id = 5, Name = "Kailushka" },
               new CampaignClient { Id = 6, Name = "Mastika Peshtera" },
               new CampaignClient { Id = 7, Name = "Slivenska perla" },
               new CampaignClient { Id = 8, Name = "Black Ram" },
               new CampaignClient { Id = 9, Name = "Sixth Sense Gin" },
               new CampaignClient { Id = 10, Name = "Straldjanska" },
               new CampaignClient { Id = 11, Name = "Abopharma" },
               new CampaignClient { Id = 12, Name = "Citroen" },
               new CampaignClient { Id = 13, Name = "Peugeot" },
               new CampaignClient { Id = 14, Name = "KiaMotors" },
               new CampaignClient { Id = 15, Name = "Italia Motors" },
               new CampaignClient { Id = 16, Name = "SFA Sofia 1" },
               new CampaignClient { Id = 17, Name = "SFA Sofia 2" },
               new CampaignClient { Id = 18, Name = "SFA Sofia 3" },
               new CampaignClient { Id = 19, Name = "SFA Sofia 4" },
               new CampaignClient { Id = 20, Name = "SFA Ruse" },
               new CampaignClient { Id = 21, Name = "SFA VTurnovo" },
               new CampaignClient { Id = 22, Name = "SFA Okazion" },
               new CampaignClient { Id = 23, Name = "SFA" },
               new CampaignClient { Id = 24, Name = "SFA Broker" },
               new CampaignClient { Id = 25, Name = "Subaru" },
               new CampaignClient { Id = 26, Name = "Honda" },
               new CampaignClient { Id = 27, Name = "Smart Electric Tech" },
               new CampaignClient { Id = 28, Name = "HMD" },
               new CampaignClient { Id = 29, Name = "MagicalFlowersBG" },
               new CampaignClient { Id = 30, Name = "eBay" },
               new CampaignClient { Id = 31, Name = "OraSi Bulgaria" },
               new CampaignClient { Id = 32, Name = "FIZI Publishing" },
               new CampaignClient { Id = 33, Name = "EMSE Publishing" },
               new CampaignClient { Id = 34, Name = "Melitta" },
               new CampaignClient { Id = 35, Name = "Pitagor School" },
               new CampaignClient { Id = 36, Name = "AYA Estate" }
            );

        }
    }
}
