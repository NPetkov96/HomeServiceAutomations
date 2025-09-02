using Operations.NoniCampaignsAttachemnts;
using static HomeService.Program;

namespace HomeService.Services
{
    public class GenerateCamoaignAttachemnt : ScheduledTask
    {
        private readonly CampaignsOperations _campaignsOperations;

        public GenerateCamoaignAttachemnt(CampaignsOperations campaignsOperations) : base(Configuration.Appsettings.GetSection("GenerateCamoaignAttachemnt").GetValue<string>("CronPattern"),
                  Configuration.Appsettings.GetSection("GenerateCamoaignAttachemnt").GetValue<bool>("ServiceActive"))
        {
            _campaignsOperations = campaignsOperations;
        }

        protected async override Task ExecuteTask()
        {
            var connectionString = Configuration.ConnectionString;
            await _campaignsOperations.RunOperation();
        }
    }
}
