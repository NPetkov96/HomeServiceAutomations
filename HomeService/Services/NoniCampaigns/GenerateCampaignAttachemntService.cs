using Operations.NoniCampaignsAttachemnts;
using static HomeService.Program;

namespace HomeService.Services.NoniCampaigns
{
    public class GenerateCampaignAttachemntService : ScheduledTask
    {
        private readonly CampaignsOperations _campaignsOperations;

        public GenerateCampaignAttachemntService(CampaignsOperations campaignsOperations) : base(Configuration.Appsettings.GetSection("GenerateCampaignAttachemntService").GetValue<string>("CronPattern"),
                  Configuration.Appsettings.GetSection("GenerateCampaignAttachemntService").GetValue<bool>("ServiceActive"))
        {
            _campaignsOperations = campaignsOperations;
        }

        protected async override Task ExecuteTask()
        {
            await _campaignsOperations.RunOperation();
        }
    }
}
