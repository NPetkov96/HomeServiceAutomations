using Extensions;
using Operations.ImotBg;
using static HomeService.Program;

namespace HomeService.Services.ImotBg
{
    public class ImotBgServise : ScheduledTask
    {
        private readonly DailyImotBgOperation _operation;

        public ImotBgServise(DailyImotBgOperation operation) : base(
            Configuration.Appsettings.GetSection("ImotBgServise").GetValue<string>("CronPattern"),
            Configuration.Appsettings.GetSection("ImotBgServise").GetValue<bool>("ServiceActive"))
        {
            _operation = operation;
        }

        protected override async Task ExecuteTask()
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
