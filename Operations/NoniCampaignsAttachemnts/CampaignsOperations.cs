using DataLayer;
using DataLayer.Models;
using Extensions;

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
                        WriteLog.Log(ex.Message, ex.StackTrace!);
                    }
                }
            }
        }
    }
}
