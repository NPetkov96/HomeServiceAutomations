using DataLayer;
using DataLayer.Models;
using OfficeOpenXml;
using System.Globalization;

namespace Operations.NoniCampaignsAttachemnts
{
    public class ExtractPropertiesByEmail
    {
        public async Task ExtractProps()
        {
            using (var db = new DataBaseContext())
            {
                var settings = db.CampaignSettings.ToDictionary(s => s.Name!, s => s.Value)!;

                var records = db.CampaignEmails
                    .Where(e => e.EmailStatus == Status.ExtractingAssets)
                    .ToList();

                var clients = db.CampaignClients
                    .ToList();

                try
                {
                    foreach (var record in records)
                    {
                        List<CampaignAsset> assets = new List<CampaignAsset>();

                        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                        using (var stream = new MemoryStream(record.Body!))
                        using (var package = new ExcelPackage(stream))
                        {
                            var worksheet = package.Workbook.Worksheets[0];

                            int rows = worksheet.Dimension.Rows;
                            int cols = worksheet.Dimension.Columns;


                            for (int row = 10; row <= rows; row++)
                            {
                                if (string.IsNullOrEmpty(worksheet.Cells[row, 2].Text)) break;
                                var asset = new CampaignAsset();
                                asset.Client = record.Client;
                                asset.CampaignId = record.CampaignId;
                                asset.Product = worksheet.Cells[7, 3].Text.Trim();

                                for (int col = 2; col <= 9; col++)
                                {
                                    string value = worksheet.Cells[row, col].Text;
                                    value.Replace(" ", "");

                                    switch (col)
                                    {
                                        case 2:
                                            asset.Platform = value;
                                            break;
                                        case 3:
                                            asset.Target = value;
                                            break;
                                        case 4:
                                            asset.Formats = value;
                                            break;
                                        case 5:
                                            asset.Optimization = value;
                                            break;
                                        case 6:
                                            string[] startFormat = { "MM/dd/yyyy", "M/d/yyyy" };
                                            asset.StartDate = DateTime.ParseExact(value, startFormat, CultureInfo.InvariantCulture, DateTimeStyles.None);
                                            break;
                                        case 7:
                                            string[] endFormtat = { "MM/dd/yyyy", "M/d/yyyy" };
                                            asset.EndDate = DateTime.ParseExact(value, endFormtat, CultureInfo.InvariantCulture, DateTimeStyles.None);
                                            break;
                                        case 8:
                                            value = value.Substring(1, value.Length - 1).Trim().Replace(",", ".").Replace(" ", "");
                                            var number = "";
                                            for (int i = 0; i < value.Length; i++)
                                            {
                                                int ascii = (int)value[i];
                                                if (ascii != 160)
                                                {
                                                    number += value[i];
                                                }
                                            }
                                            decimal amount = decimal.TryParse(number, out var parsed) ? parsed : 0;
                                            asset.Budget = amount;
                                            break;
                                    }
                                }
                                asset.Period = (asset.EndDate!.Value - asset.StartDate!.Value).Days.ToString();
                                assets.Add(asset);
                            }
                            record.Assets = assets;
                            record.EmailStatus = Status.GeneratingFile;
                            db.SaveChanges();
                        }
                    }
                }
                catch (Exception ex)
                {
                    File.AppendAllText(settings["ErrorFilePth"]!, $"[{DateTime.Now}] Грешка: {ex.Message}\n");
                    Console.WriteLine(ex.Message);
                }

            }
        }
    }
}
