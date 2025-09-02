using DataLayer;
using DataLayer.Models;

namespace Operations.NoniCampaignsAttachemnts
{
    public class CampaignsOperations
    {
        //private readonly DataBaseContext _dbContext;
        private readonly Emails _emailReader;
        private readonly ExtractPropertiesByEmail _extractPropertiesByEmail;
        private readonly CreateCampaign _createCampaign;

        public CampaignsOperations(Emails emailReader, ExtractPropertiesByEmail extractPropertiesByEmail, CreateCampaign createCampaign)
        {

            //_dbContext = dbContext;
            _emailReader = emailReader;
            _extractPropertiesByEmail = extractPropertiesByEmail;
            _createCampaign = createCampaign;
        }

        public async Task RunOperation()
        {
            //using (var context = new DataBaseContext())
            //{
            //    var clients = context.CampaignClients.ToList();
            //    var platforms = context.CampaignPlatforms.ToList();

            //    foreach (var client in clients)
            //    {
            //        Console.WriteLine($"Client: {client.Name}");

            //        foreach (var platform in platforms)
            //        {
            //            double percentFee = -1;

            //            while (percentFee < 0)
            //            {
            //                Console.Write($"→ Platform : {platform.Name}: ");
            //                var input = Console.ReadLine();

            //                if (double.TryParse(input, out percentFee) && percentFee >= 0)
            //                {
            //                    var newRecord = new CampaignClientPlatform
            //                    {
            //                        ClientId = client.Id,
            //                        PlatformId = platform.Id,
            //                        PercentFee = percentFee
            //                    };

            //                    context.CampaignClientPlatforms.Add(newRecord);
            //                }
            //            }
            //        }

            //        context.SaveChanges();
            //    }

            //    Console.WriteLine("Done");
            //}

            using (var db = new DataBaseContext())
            {
                var settings = db.CampaignSettings.ToDictionary(s => s.Name!, s => s.Value)!;
                await _emailReader.ReadEmails();

                if (db.CampaignEmails.Any(e => e.EmailStatus == Status.ExtractingAssets))
                {
                    try
                    {
                        await _extractPropertiesByEmail.ExtractProps();
                        await _createCampaign.Create();
                        await _emailReader.SendEmailWithAttachment();
                    }
                    catch (Exception ex)
                    {
                        var logPath = Path.Combine(db.Settings.FirstOrDefault(s => s.Name == "LogsPath")!.Value!, $"{DateTime.Now.ToString("yyyy-MM-dd")}-Logs.txt");
                        File.AppendAllText(logPath, $"[{DateTime.Now}] Грешка: {ex.Message}\n {ex.StackTrace}\n");
                    }
                }
            }
        }
    }
}
