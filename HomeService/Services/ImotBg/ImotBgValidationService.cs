
using DataLayer;
using Extensions;
using Operations.ImotBg;
using static HomeService.Program;

namespace HomeService.Services.ImotBg
{
    public class ImotBgValidationService : ScheduledTask
    {
        private readonly ImotBgValidation _imotBgValidation;

        public ImotBgValidationService(ImotBgValidation imotBgValidation) : base(Configuration.Appsettings.GetSection("ImotBgValidationService").GetValue<string>("CronPattern"),
                 Configuration.Appsettings.GetSection("ImotBgValidationService").GetValue<bool>("ServiceActive"))
        {
            this._imotBgValidation = imotBgValidation;
        }

        protected async override Task ExecuteTask()
        {
            using (var db = new DataBaseContext())
            {
                await _imotBgValidation.ValidateData(db);
                WriteLog.Log("Validation successful!");
            }
        }
    }
}
