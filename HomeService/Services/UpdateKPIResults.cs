using DataLayer;
using Operations.UpdateKPIResults;
using static HomeService.Program;

namespace HomeService.Services
{
    public class UpdateKPIResults : ScheduledTask
    {
        private readonly UpdateKPI _updateKPIResults;

        public UpdateKPIResults(UpdateKPI updateKPIResults) : base(Configuration.Appsettings.GetSection("UpdateKPIResults").GetValue<string>("CronPattern"),
                  Configuration.Appsettings.GetSection("UpdateKPIResults").GetValue<bool>("ServiceActive"))
        {
            _updateKPIResults = updateKPIResults;
        }

        protected override async Task ExecuteTask()
        {
            using (var db = new DataBaseContext())
            {
                var settings = db.CampaignSettings.ToDictionary(s => s.Name!, s => s.Value)!;
                try
                {
                    await _updateKPIResults.Update();

                    var newTime = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss-f");
                    File.AppendAllText(settings["ErrorFilePth"]!, $"[{DateTime.Now}] Successfully updated KPI!\n");
                }
                catch (Exception ex)
                {
                    File.AppendAllText(settings["ErrorFilePth"]!, $"[{DateTime.Now}] Error: {ex.Message}\nInner: {ex.InnerException?.Message}\nStack: {ex.StackTrace}\n");
                }
            }
        }
    }
}
