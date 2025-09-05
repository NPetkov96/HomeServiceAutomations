using DataLayer;
using Extensions;
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
            try
            {
                await _updateKPIResults.Update();
                WriteLog.Log("Successfully updated KPI!");
            }
            catch (Exception ex)
            {
                WriteLog.Log(ex.Message, ex.StackTrace!);
            }
        }
    }
}
