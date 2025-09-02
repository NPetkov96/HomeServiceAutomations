using DataLayer;
using DataLayer.Models;

namespace Operations.NoniCampaignsAttachemnts
{
    public class CampaignsOperations
    {
        //private readonly DataBaseContext _dbContext;
        private readonly EmailReader _emailReader;
        private readonly ExtractPropertiesByEmail _extractPropertiesByEmail;
        private readonly CreateCampaign _createCampaign;

        public CampaignsOperations(EmailReader emailReader, ExtractPropertiesByEmail extractPropertiesByEmail, CreateCampaign createCampaign)
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

            using (var _dbContext = new DataBaseContext())
            {
                var settings = _dbContext.CampaignSettings.ToDictionary(s => s.Name!, s => s.Value)!;
                await _emailReader.ReadEmails();

                if (_dbContext.CampaignEmails.Any(e => e.EmailStatus == Status.ExtractingAssets))
                {
                    try
                    {
                        await _extractPropertiesByEmail.ExtractProps();
                        await _createCampaign.Create();
                        await _emailReader.SendEmailWithAttachment();
                    }
                    catch (Exception ex)
                    {
                        File.AppendAllText(settings["ErrorFilePth"]!, $"[{DateTime.Now}] {ex.StackTrace}\n");
                    }
                }
            }
        }
    }
}
