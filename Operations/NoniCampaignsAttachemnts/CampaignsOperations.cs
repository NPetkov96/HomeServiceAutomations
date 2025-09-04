using DataLayer;
using DataLayer.Models;

namespace Operations.NoniCampaignsAttachemnts
{
    public class CampaignsOperations
    {
        private readonly Emails _emailReader;
        private readonly ExtractPropertiesByEmail _extractPropertiesByEmail;
        private readonly CreateCampaign _createCampaign;

        public CampaignsOperations(Emails emailReader, ExtractPropertiesByEmail extractPropertiesByEmail, CreateCampaign createCampaign)
        {
            _emailReader = emailReader;
            _extractPropertiesByEmail = extractPropertiesByEmail;
            _createCampaign = createCampaign;
        }

        public async Task RunOperation()
        {
            
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
