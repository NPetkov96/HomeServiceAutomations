using DataLayer;
using Extensions;
using Operations.Catheters;
using static HomeService.Program;

namespace HomeService.Services.MedSestri
{
    public class ExpiredCathetersService : ScheduledTask
    {

        private readonly ExpiredCathetersOperation _operation;

        public ExpiredCathetersService(ExpiredCathetersOperation operation) : base(Configuration.Appsettings.GetSection("ExpiredCathetersService").GetValue<string>("CronPattern"),
                  Configuration.Appsettings.GetSection("ExpiredCathetersService").GetValue<bool>("ServiceActive"))
        {
            _operation = operation;
        }

        protected async override Task ExecuteTask()
        {
            try
            {
                await _operation.Run();
            }
            catch (Exception ex)
            {
                WriteLog.Log(ex.Message, ex.StackTrace!);
            }
        }
    }
}
