using DataLayer;
using DataLayer.Models;
using Extensions;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

namespace Operations.NoniCampaignsAttachemnts
{
    public class CreateCampaign
    {
        public async Task Create()
        {
            using (var db = new DataBaseContext())
            {
                try
                {

                    var settings = db.CampaignSettings.ToDictionary(s => s.Name!, s => s.Value)!;

                    var records = db.CampaignEmails
                                   .Where(r => r.EmailStatus == Status.GeneratingFile)
                                   .Include(x => x.Assets)
                                   .ToList();

                    var cleintsPlatforms = db.CampaignClientPlatforms
                                  .Include(x => x.Client)
                                  .Include(x => x.Platform)
                                  .ToList();

                    var platforms = db.CampaignPlatforms.ToList();

                    foreach (var record in records)
                    {
                        int assets = record!.Assets.Count;
                        var template = $"C:\\HomeService\\Campaigns\\CampaignTemplates\\Template-{assets}.xlsx";
                        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;


                        using (var package = new ExcelPackage(new FileInfo(template)))
                        {
                            var ws = package.Workbook.Worksheets[0];

                            var earliestDay = record.Assets != null && record.Assets.Any()
                                ? record.Assets
                                    .Min(x => x.StartDate)?.ToString("dd.MM.yyyy").Trim() ?? string.Empty
                                : string.Empty;

                            var latestDay = record.Assets != null && record.Assets.Any()
                                ? record.Assets
                                    .Max(x => x.EndDate)?.ToString("dd.MM.yyyy").Trim() ?? string.Empty
                                : string.Empty;

                            string clientName = record.Assets!.FirstOrDefault()!.Client!;
                            string campaignId = record.Assets!.FirstOrDefault()!.CampaignId!;
                            string dateTimeNow = DateTime.Now.ToString("dd.MM.yyyy");

                            ws.Cells[7, 4].Value = clientName;
                            ws.Cells[8, 4].Value = "Internet";
                            ws.Cells[9, 4].Value = "Argent Group";
                            ws.Cells[10, 4].Value = dateTimeNow;
                            ws.Cells[11, 4].Value = $"{earliestDay} - {latestDay}";
                            ws.Cells[13, 4].Value = campaignId;

                            int startRow = 20;
                            int startCol = 2;
                            int[] succesCol = { 2, 3, 4, 5, 10, 11, 13, 17, 19 };
                            string[] avaiablePlatformsForKPI = { "Facebook & Instagram", "Instagram", "Facebook" };

                            var recordAssets = record.Assets;
                            int assetCounter = 0;

                            for (int i = startRow; i < startRow + assets; i++)
                            {
                                CampaignAsset asset = recordAssets![assetCounter++];

                                var currentFee = cleintsPlatforms
                                .FirstOrDefault(c => c.Client.Name.ToString().Replace(" ", "").ToLower() == asset.Client.ToString().Replace(" ", "").ToLower() &&
                                                c.Platform.Name.ToString().Replace(" ", "").ToLower() == asset.Platform!.ToString().Replace(" ", "").ToLower())!.PercentFee;


                                var currentPlatform = platforms.FirstOrDefault(p => p.Name == asset.Platform);
                                string kpi = null;

                                switch (currentPlatform!.Name)
                                {
                                    case "Facebook":
                                    case "Instagram":
                                    case "Facebook & Instagram":
                                        switch (asset.Optimization)
                                        {
                                            case "Traffic":
                                            case "Brand Awareness":
                                            case "Reach":
                                                kpi = currentPlatform.CPC;
                                                break;
                                            default:
                                                kpi = currentPlatform.CostPerPostEngagment;
                                                break;
                                        }
                                        break;

                                    case "Google":
                                    case "EasyAds":
                                        kpi = currentPlatform.CPC;
                                        break;

                                    case "YouTube":
                                        kpi = currentPlatform.CostPerPostEngagment;
                                        break;

                                    case "TikTok":
                                        switch (asset.Optimization)
                                        {
                                            case "Traffic":
                                                kpi = currentPlatform.CPC;
                                                break;
                                            default:
                                                kpi = currentPlatform.CostPerPostEngagment;
                                                break;
                                        }
                                        break;

                                }

                                for (int c = startCol; c < 20; c++)
                                {
                                    if (succesCol.Contains(c))
                                    {
                                        switch (c)
                                        {
                                            case 2:
                                                ws.Cells[i, c].Value = asset.Platform;
                                                break;
                                            case 3:
                                                ws.Cells[i, c].Value = asset.Target;
                                                break;
                                            case 4:
                                                ws.Cells[i, c].Value = asset.Optimization;
                                                break;
                                            case 5:
                                                ws.Cells[i, c].Value = asset.Formats;
                                                break;
                                            case 10:
                                                ws.Cells[i, c].Value = asset.StartDate;
                                                break;
                                            case 11:
                                                ws.Cells[i, c].Value = asset.EndDate;
                                                break;
                                            case 13:
                                                if (kpi != null) ws.Cells[i, c].Value = kpi;
                                                break;
                                            case 17:
                                                ws.Cells[i, c].Value = currentFee / 100;
                                                break;
                                            case 19:
                                                ws.Cells[i, c].Value = asset.Budget;
                                                break;
                                        }
                                    }
                                }
                            }

                            var currentFolderPath = Path.Combine(settings["SavingAttachmentsPath"]!, DateTime.Now.ToString("yyyy-MM-dd"));
                            if (!Directory.Exists(currentFolderPath))
                            {
                                Directory.CreateDirectory(currentFolderPath);
                                WriteLog.Log("Campaign file is generated");
                            }

                            var uniqueId = Guid.NewGuid().ToString().Substring(0, 5);
                            var fileName = $"{campaignId}-{clientName}-{dateTimeNow}-{uniqueId}.xlsx";
                            var filePath = Path.Combine(currentFolderPath, fileName);
                            package.SaveAs(new FileInfo(filePath));

                            if (File.Exists(filePath))
                            {
                                record.EmailStatus = Status.SendingFile;
                                record.AttachmentPath = filePath;
                            }
                            db.SaveChanges();
                        }
                    }
                }
                catch (Exception ex)
                {
                    WriteLog.Log(ex.Message, ex.StackTrace!);
                }
            }
        }
    }
}
