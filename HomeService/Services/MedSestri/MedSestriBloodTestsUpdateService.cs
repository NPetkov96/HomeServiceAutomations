using Operations.BloodTetsUpdate;
using Operations.UpdateKPIResults;
using static HomeService.Program;

namespace HomeService.Services.MedSestri
{
    public class MedSestriBloodTestsUpdateService : ScheduledTask
    {
        private readonly UpdateBloodTestsOperation _bloodTests;
        public MedSestriBloodTestsUpdateService(UpdateBloodTestsOperation bloodTests) : base(Configuration.Appsettings.GetSection("MedSestriBloodTestsUpdateService").GetValue<string>("CronPattern"),
                  Configuration.Appsettings.GetSection("MedSestriBloodTestsUpdateService").GetValue<bool>("ServiceActive"))
        {
            _bloodTests = bloodTests;
        }

        protected override async Task ExecuteTask()
        {
            await _bloodTests.Run();
        }
    }
}
