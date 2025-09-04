using DataLayer;
using Operations.UpdateKPIResults;
using static HomeService.Program;

namespace HomeService.Services
{
    public class UpdateKPIResultsService : ScheduledTask
    {
        private readonly UpdateKPI _updateKPIResults;

        public UpdateKPIResultsService(UpdateKPI updateKPIResults) : base(Configuration.Appsettings.GetSection("UpdateKPIResultsService").GetValue<string>("CronPattern"),
                  Configuration.Appsettings.GetSection("UpdateKPIResultsService").GetValue<bool>("ServiceActive"))
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

                    var logPath = Path.Combine(db.Settings.FirstOrDefault(s => s.Name == "LogsPath")!.Value!, $"{DateTime.Now.ToString("yyyy-MM-dd")}-Logs.txt");
                    File.AppendAllText(logPath, $"[{DateTime.Now}] Successfully updated KPI!\n");
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
